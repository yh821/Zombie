using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/// <summary>
/// Fog of War system needs 3 components in order to work:
/// - Fog of War system that will create a height map of your scene and perform all the updates (this class).
/// - Fog of War Image Effect on the camera that will be displaying the fog of war.
/// - Fog of War Revealer on one or more game objects in the world.
/// </summary>

[AddComponentMenu("Fog of War/System")]
public class FOWSystem : MonoBehaviour
{
    public enum LOSChecks
    {
        None,
        OnlyOnce,
        EveryUpdate,
    }

    public class Revealer
    {
        public bool isActive = false;
        public LOSChecks los = LOSChecks.None;
        public Vector3 pos = Vector3.zero;
        public float inner = 0f;
        public float outer = 0f;
        public bool[] cachedBuffer;
        public int cachedSize = 0;
        public int cachedX = 0;
        public int cachedY = 0;
    }

    public enum State
    {
        /// <summary>
        /// 混合
        /// </summary>
        Blending,
        /// <summary>
        /// 更新
        /// </summary>
        NeedUpdate,
        /// <summary>
        /// 更新Tex0
        /// </summary>
        UpdateTexture0,
        /// <summary>
        /// 更新Tex1
        /// </summary>
        UpdateTexture1,
    }

    static public FOWSystem instance;

    // Height map used for visibility checks. Integers are used instead of floats as integer checks are significantly faster.
    protected short[,] mHeights;
    protected short[,] mMapHeights;
    protected Transform mTrans;
    protected Vector3 mOrigin = Vector3.zero;
    protected Vector3 mSize = Vector3.one;

    // Revealers that the thread is currently working with
    static List<Revealer> mRevealers = new List<Revealer>();

    // Revealers that have been added since last update
    static Queue<Revealer> mAdded = new Queue<Revealer>();

    // Revealers that have been removed since last update
    static Queue<Revealer> mRemoved = new Queue<Revealer>();

    // Color buffers -- prepared on the worker thread.
    protected Color32[] mBuffer0;
    protected Color32[] mBuffer1;
    protected Color32[] mBuffer2;//模糊缓冲区

    // Two textures -- we'll be blending between them in the shader
    protected Texture2D mTexture0;
    protected Texture2D mTexture1;

    // Whether some color buffer is ready to be uploaded to VRAM
    protected float mBlendFactor = 0f;
    protected float mNextUpdate = 0f;
    protected int mScreenHeight = 0;
    protected State mState = State.Blending;

    private Vector3 mRevealerPosition;
    private Vector3 mMapOrigin;

    Thread mThread;

    /// <summary>
    /// Size of your world in units. For example, if you have a 256x256 terrain, then just leave this at '256'.
    /// </summary>

    public int worldSize = 32;

    /// <summary>
    /// Size of the fog of war texture. Higher resolution will result in more precise fog of war, at the cost of performance.
    /// </summary>
    protected int textureSize;
    public int pixScale = 2;
    public int mapSize = 2048;

    /// <summary>
    /// How frequently the visibility checks get performed.
    /// </summary>

    public float updateFrequency = 0.1f;

    /// <summary>
    /// How long it takes for textures to blend from one to another.
    /// </summary>

    public float textureBlendTime = 0.5f;

    /// <summary>
    /// How many blur iterations will be performed. More iterations result in smoother edges.
    /// Blurring happens on a separate thread and does not affect performance.
    /// </summary>

    public int blurIterations = 2;

    /// <summary>
    /// What is the lowest and highest height of your world? Revealers below the X will not reveal anything,
    /// while revealers above Y will reveal everything around them.
    /// </summary>

    public Vector2 heightRange = new Vector2(0f, 32f);

    /// <summary>
    /// Mask used for raycasting to determine whether there is an obstruction or not.
    /// </summary>

    public LayerMask raycastMask;

    /// <summary>
    /// 检测射线球半径
    /// </summary>
    /// Radius of the sphere if using SphereCast. If 0, line-based raycasting will be used instead.
    public float raycastRadius = 0f;

    /// <summary>
    /// 允许高度误差
    /// </summary>
    /// Allows for some height variance when performing line-of-sight checks.
    public float margin = 0.4f;

    /// <summary>
    /// If debugging is enabled, the time it takes to calculate the fog of war will be shown in the log window.
    /// </summary>

    public bool debug = false;

    /// <summary>
    /// The fog texture we're blending from.
    /// </summary>

    public Texture2D texture0 { get { return mTexture0; } }

    /// <summary>
    /// The fog texture we're blending to.
    /// </summary>

    public Texture2D texture1 { get { return mTexture1; } }

    /// <summary>
    /// Factor used to blend between the two textures.
    /// </summary>

    public float blendFactor { get { return mBlendFactor; } }

    /// <summary>
    /// Create a new fog revealer.
    /// </summary>

    static public Revealer CreateRevealer()
    {
        Revealer rev = new Revealer();
        rev.isActive = false;
        lock (mAdded) mAdded.Enqueue(rev);
        return rev;
    }

    /// <summary>
    /// Delete the specified revealer.
    /// </summary>

    static public void DeleteRevealer(Revealer rev) { lock (mRemoved) mRemoved.Enqueue(rev); }

    /// <summary>
    /// Set the instance.
    /// </summary>

    void Awake() { instance = this; }

    /// <summary>
    /// Generate the height grid.
    /// </summary>

    void Start()
    {
        InitMapHeightInfo();

        mTrans = transform;
        textureSize = worldSize / pixScale;
        mSize = new Vector3(worldSize, heightRange.y - heightRange.x, worldSize);

        mOrigin = mTrans.position;
        mOrigin.x -= worldSize * 0.5f;
        mOrigin.z -= worldSize * 0.5f;

        mMapOrigin = mTrans.position;
        mMapOrigin.x -= mapSize * 0.5f;
        mMapOrigin.z -= mapSize * 0.5f;

        mRevealerPosition = mTrans.position - mOrigin;

        int size = textureSize * textureSize;
        mBuffer0 = new Color32[size];
        mBuffer1 = new Color32[size];
        mBuffer2 = new Color32[size];

        // Create the height grid
        //CreateGrid();

        // Update the fog of war's visibility so that it's updated right away
        UpdateBuffer();
        UpdateTexture();
        mNextUpdate = Time.time + updateFrequency;

        // Add a thread update function -- all visibility checks will be done on a separate thread
        mThread = new Thread(ThreadUpdate);//另起线程更新缓冲区
        mThread.Start();
    }

    /// <summary>
    /// Ensure that the thread gets terminated.
    /// </summary>

    void OnDestroy()
    {
        if (mThread != null)
        {
            mThread.Abort();
            while (mThread.IsAlive) Thread.Sleep(1);
            mThread = null;
        }
    }

    /// <summary>
    /// Is it time to update the visibility? If so, flip the switch.
    /// </summary>

    void Update()
    {
        if (textureBlendTime > 0f)
        {
            mBlendFactor = Mathf.Clamp01(mBlendFactor + Time.deltaTime / textureBlendTime);
        }
        else mBlendFactor = 1f;

        if (mState == State.Blending)
        {
            float time = Time.time;

            if (mNextUpdate < time)
            {
                mNextUpdate = time + updateFrequency;
                mState = State.NeedUpdate;
            }
        }
        else if (mState != State.NeedUpdate)
        {
            UpdateTexture();//主线程更新texture
        }
    }

    float mElapsed = 0f;

    /// <summary>
    /// If it's time to update, do so now.
    /// </summary>

    void ThreadUpdate()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        while (true)
        {
            if (mState == State.NeedUpdate)
            {
                sw.Reset();
                sw.Start();
                UpdateBuffer();
                sw.Stop();
                if (debug) Debug.Log(sw.ElapsedMilliseconds);
                mElapsed = 0.001f * (float)sw.ElapsedMilliseconds;
                mState = State.UpdateTexture0;
            }
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// Show the area covered by the fog of war.
    /// </summary>

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(0f, (heightRange.x + heightRange.y) * 0.5f, 0f),
            new Vector3(worldSize, heightRange.y - heightRange.x, worldSize));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(0f, (heightRange.x + heightRange.y) * 0.5f, 0f),
            new Vector3(mapSize, heightRange.y - heightRange.x, mapSize));
    }

    /// <summary>
    /// Determine if the specified point is visible or not using line-of-sight checks.
    /// </summary>

    bool IsVisible(int sx, int sy, int fx, int fy, float outer, int sightHeight, int variance)
    {
        int dx = Mathf.Abs(fx - sx);
        int dy = Mathf.Abs(fy - sy);
        int ax = sx < fx ? 1 : -1;
        int ay = sy < fy ? 1 : -1;
        int dir = dx - dy;

        float sh = sightHeight;
        float fh = mHeights[fx, fy];

        float invDist = 1f / outer;
        float lerpFactor = 0f;

        while (true)
        {
            if (sx == fx && sy == fy) return true;

            int xd = fx - sx;
            int yd = fy - sy;

            // If the sampled height is higher than expected, then the point must be obscured
            lerpFactor = invDist * Mathf.Sqrt(xd * xd + yd * yd);
            if (mHeights[sx, sy] > Mathf.Lerp(fh, sh, lerpFactor) + variance) return false;

            int dir2 = dir << 1;

            if (dir2 > -dy)
            {
                dir -= dy;
                sx += ax;
            }

            if (dir2 < dx)
            {
                dir += dx;
                sy += ay;
            }
        }
    }

    /// <summary>
    /// Convert the specified height into the internal integer representation. Integer checks are much faster than floats.
    /// </summary>

    public short WorldToGridHeight(float height)
    {
        int val = Mathf.RoundToInt(height / mSize.y * 255f);
        return (short)Mathf.Clamp(val, 0, 255);
    }

    /// <summary>
    /// Create the heightmap grid using the default technique (raycasting).
    /// </summary>
    protected virtual void CreateGrid()
    {
        Vector3 pos = mOrigin;
        pos.y += mSize.y;
        float texToWorld = (float)worldSize / textureSize;
        bool useSphereCast = raycastRadius > 0f;
        mHeights = new short[textureSize, textureSize];

        for (int z = 0; z < textureSize; ++z)
        {
            pos.z = mOrigin.z + z * texToWorld;

            for (int x = 0; x < textureSize; ++x)
            {
                pos.x = mOrigin.x + x * texToWorld;

                RaycastHit hit;

                if (useSphereCast)
                {
                    if (Physics.SphereCast(new Ray(pos, Vector3.down), raycastRadius, out hit, mSize.y, raycastMask))
                    {
                        mHeights[x, z] = WorldToGridHeight(pos.y - hit.distance - raycastRadius);
                        continue;
                    }
                }
                else if (Physics.Raycast(new Ray(pos, Vector3.down), out hit, mSize.y, raycastMask))
                {
                    mHeights[x, z] = WorldToGridHeight(pos.y - hit.distance);
                    continue;
                }
                mHeights[x, z] = 0;
            }
        }
    }

    protected void InitMapHeightInfo()
    {
        byte[] bytes = CResourceSys.Instance.LoadTable("mapheight.bytes");
        mapSize = BitConverter.ToInt16(bytes, 0);
        mMapHeights = new short[mapSize, mapSize];
        mHeights = new short[worldSize, worldSize];
        const int FIRST_INDEX = 2;
        int index;
        for (int y = 0, r = 0; y < mapSize; ++y)
        {
            for (int x = 0; x < mapSize; ++x, ++r)
            {
                index = FIRST_INDEX + (x + y * mapSize) * 2;
                mMapHeights[x, y] = BitConverter.ToInt16(bytes, index);
            }
        }
    }

    /// <summary>
    /// Update the fog of war's visibility.
    /// </summary>

    void UpdateBuffer()
    {
        // Add all items scheduled to be added
        if (mAdded.Count > 0)
        {
            lock (mAdded)
            {
                while (mAdded.Count > 0)
                {
                    mRevealers.Add(mAdded.Dequeue());
                }
            }
        }

        // Remove all items scheduled for removal
        if (mRemoved.Count > 0)
        {
            lock (mRemoved)
            {
                while (mRemoved.Count > 0)
                {
                    mRevealers.Remove(mRemoved.Dequeue());
                }
            }
        }

        // Use the texture blend time, thus estimating the time this update will finish
        // Doing so helps avoid visible changes in blending caused by the blended result being X milliseconds behind.
        float factor = (textureBlendTime > 0f) ? Mathf.Clamp01(mBlendFactor + mElapsed / textureBlendTime) : 1f;

        //清除缓冲区的红色通道(用于当前可见性的通道——它将在后面更新)
        for (int i = 0, imax = mBuffer0.Length; i < imax; ++i)
        {
            mBuffer0[i] = Color32.Lerp(mBuffer0[i], mBuffer1[i], factor);
            mBuffer1[i].r = 0;
        }

        // For conversion from world coordinates to texture coordinates
        float worldToTex = (float)textureSize / worldSize;

        // Update the visibility buffer, one revealer at a time
        for (int i = 0; i < mRevealers.Count; ++i)
        {
            Revealer rev = mRevealers[i];
            if (!rev.isActive) continue;

            if (rev.los == LOSChecks.None)
            {
                RevealUsingRadius(rev, worldToTex);
            }
            else if (rev.los == LOSChecks.OnlyOnce)
            {
                RefreshMapHeight(rev);
                RevealUsingCache(rev, worldToTex);
            }
            else
            {
                RefreshMapHeight(rev);
                RevealUsingLOS(rev, worldToTex);
            }
        }

        //模糊
        for (int i = 0; i < blurIterations; ++i) BlurVisibility();

        //通过更新绿色通道来显示地图最大的红色通道。
        //RevealMap();
    }

    /// <summary>
    /// 只算半径，不算遮挡
    /// </summary>
    /// The fastest form of visibility updates -- radius-based, no line of sights checks.
    void RevealUsingRadius(Revealer r, float worldToTex)
    {
        // Position relative to the fog of war
        Vector3 pos = r.pos - mOrigin;

        // Coordinates we'll be dealing with
        int xmin = Mathf.RoundToInt((pos.x - r.outer) * worldToTex);
        int ymin = Mathf.RoundToInt((pos.z - r.outer) * worldToTex);
        int xmax = Mathf.RoundToInt((pos.x + r.outer) * worldToTex);
        int ymax = Mathf.RoundToInt((pos.z + r.outer) * worldToTex);

        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, textureSize - 1);
        cy = Mathf.Clamp(cy, 0, textureSize - 1);

        int radius = Mathf.RoundToInt(r.outer * r.outer * worldToTex * worldToTex);

        for (int y = ymin; y < ymax; ++y)
        {
            if (y > -1 && y < textureSize)
            {
                int yw = y * textureSize;

                for (int x = xmin; x < xmax; ++x)
                {
                    if (x > -1 && x < textureSize)
                    {
                        int xd = x - cx;
                        int yd = y - cy;
                        int dist = xd * xd + yd * yd;

                        // Reveal this pixel
                        if (dist < radius) mBuffer1[x + yw].r = 255;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 内外圈渲染
    /// </summary>
    /// Reveal the map around the revealer performing line-of-sight checks.
    void RevealUsingLOS(Revealer r, float worldToTex)
    {
        // Position relative to the fog of war
        Vector3 pos = mRevealerPosition;//r.pos - mOrigin;

        // Coordinates we'll be dealing with
        int xmin = Mathf.RoundToInt((pos.x - r.outer) * worldToTex);
        int ymin = Mathf.RoundToInt((pos.z - r.outer) * worldToTex);
        int xmax = Mathf.RoundToInt((pos.x + r.outer) * worldToTex);
        int ymax = Mathf.RoundToInt((pos.z + r.outer) * worldToTex);

        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, textureSize - 1);
        cy = Mathf.Clamp(cy, 0, textureSize - 1);

        int minRange = Mathf.RoundToInt(r.inner * r.inner * worldToTex * worldToTex);
        int maxRange = Mathf.RoundToInt(r.outer * r.outer * worldToTex * worldToTex);
        //int gh = WorldToGridHeight(r.pos.y);
        int variance = Mathf.RoundToInt(Mathf.Clamp01(margin / (heightRange.y - heightRange.x)) * 255);
        Color32 white = new Color32(255, 255, 255, 255);

        for (int y = ymin; y < ymax; ++y)
        {
            if (y > -1 && y < textureSize)
            {
                for (int x = xmin; x < xmax; ++x)
                {
                    if (x > -1 && x < textureSize)
                    {
                        int xd = x - cx;
                        int yd = y - cy;
                        int dist = xd * xd + yd * yd;
                        int index = x + y * textureSize;

                        if (dist < minRange || (cx == x && cy == y))
                        {
                            mBuffer1[index] = white;
                        }
                        else if (dist < maxRange)
                        {
                            Vector2 v = new Vector2(xd, yd);
                            v.Normalize();
                            v *= r.inner;

                            int sx = cx + Mathf.RoundToInt(v.x);
                            int sy = cy + Mathf.RoundToInt(v.y);

                            if (sx > -1 && sx < textureSize &&
                                sy > -1 && sy < textureSize &&
                                IsVisible(sx, sy, x, y, Mathf.Sqrt(dist), 16, variance))//临时手动设置角色身高
                            {
                                mBuffer1[index] = white;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 通过缓存显示
    /// </summary>
    /// Reveal the map using a cached result.
    void RevealUsingCache(Revealer r, float worldToTex)
    {
        if (r.cachedBuffer == null) RevealIntoCache(r, worldToTex);

        Color32 white = new Color32(255, 255, 255, 255);

        for (int y = r.cachedY, ymax = r.cachedY + r.cachedSize; y < ymax; ++y)
        {
            if (y > -1 && y < textureSize)
            {
                int by = y * textureSize;
                int cy = (y - r.cachedY) * r.cachedSize;

                for (int x = r.cachedX, xmax = r.cachedX + r.cachedSize; x < xmax; ++x)
                {
                    if (x > -1 && x < textureSize)
                    {
                        int cachedIndex = x - r.cachedX + cy;

                        if (r.cachedBuffer[cachedIndex])
                        {
                            mBuffer1[x + by] = white;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 计算显示缓存
    /// </summary>
    /// Create the cached visibility result.
    void RevealIntoCache(Revealer r, float worldToTex)
    {
        // Position relative to the fog of war
        Vector3 pos = r.pos - mOrigin;

        // Coordinates we'll be dealing with
        int xmin = Mathf.RoundToInt((pos.x - r.outer) * worldToTex);
        int ymin = Mathf.RoundToInt((pos.z - r.outer) * worldToTex);
        int xmax = Mathf.RoundToInt((pos.x + r.outer) * worldToTex);
        int ymax = Mathf.RoundToInt((pos.z + r.outer) * worldToTex);

        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, textureSize - 1);
        cy = Mathf.Clamp(cy, 0, textureSize - 1);

        // Create the buffer to reveal into
        int size = Mathf.RoundToInt(xmax - xmin);
        r.cachedBuffer = new bool[size * size];
        r.cachedSize = size;
        r.cachedX = xmin;
        r.cachedY = ymin;

        // The buffer should start off clear
        for (int i = 0, imax = size * size; i < imax; ++i) r.cachedBuffer[i] = false;

        int minRange = Mathf.RoundToInt(r.inner * r.inner * worldToTex * worldToTex);
        int maxRange = Mathf.RoundToInt(r.outer * r.outer * worldToTex * worldToTex);
        int variance = Mathf.RoundToInt(Mathf.Clamp01(margin / (heightRange.y - heightRange.x)) * 255);
        int gh = WorldToGridHeight(r.pos.y);

        for (int y = ymin; y < ymax; ++y)
        {
            if (y > -1 && y < textureSize)
            {
                for (int x = xmin; x < xmax; ++x)
                {
                    if (x > -1 && x < textureSize)
                    {
                        int xd = x - cx;
                        int yd = y - cy;
                        int dist = xd * xd + yd * yd;

                        if (dist < minRange || (cx == x && cy == y))
                        {
                            r.cachedBuffer[(x - xmin) + (y - ymin) * size] = true;
                        }
                        else if (dist < maxRange)
                        {
                            Vector2 v = new Vector2(xd, yd);
                            v.Normalize();
                            v *= r.inner;

                            int sx = cx + Mathf.RoundToInt(v.x);
                            int sy = cy + Mathf.RoundToInt(v.y);

                            if (sx > -1 && sx < textureSize &&
                                sy > -1 && sy < textureSize &&
                                IsVisible(sx, sy, x, y, Mathf.Sqrt(dist), gh, variance))
                            {
                                r.cachedBuffer[(x - xmin) + (y - ymin) * size] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    void RefreshMapHeight(Revealer r)
    {
        Vector3 pos = r.pos - mMapOrigin;
        int sx = Mathf.RoundToInt(pos.x - worldSize * 0.5f) + 1;
        int sy = Mathf.RoundToInt(pos.z - worldSize * 0.5f);
        for (int wy = 0, my = sy, ylen = worldSize + sy; my < ylen; my++, wy++)
        {
            for (int wx = 0, mx = sx, xlen = worldSize + sx; mx < xlen; mx++, wx++)
            {
                mHeights[wx, wy] = mMapHeights[mx, my];
            }
        }
    }

    /// <summary>
    /// 模糊算法，混合两张图
    /// </summary>
    void BlurVisibility()
    {
        Color32 c;

        for (int y = 0; y < textureSize; ++y)
        {
            int yw = y * textureSize;
            int yw0 = (y - 1);
            if (yw0 < 0) yw0 = 0;
            int yw1 = (y + 1);
            if (yw1 == textureSize) yw1 = y;

            yw0 *= textureSize;
            yw1 *= textureSize;

            for (int x = 0; x < textureSize; ++x)
            {
                int x0 = (x - 1);
                if (x0 < 0) x0 = 0;
                int x1 = (x + 1);
                if (x1 == textureSize) x1 = x;

                int index = x + yw;
                int val = mBuffer1[index].r;

                val += mBuffer1[x0 + yw].r;
                val += mBuffer1[x1 + yw].r;
                val += mBuffer1[x + yw0].r;
                val += mBuffer1[x + yw1].r;

                val += mBuffer1[x0 + yw0].r;
                val += mBuffer1[x1 + yw0].r;
                val += mBuffer1[x0 + yw1].r;
                val += mBuffer1[x1 + yw1].r;

                c = mBuffer2[index];
                c.r = (byte)(val / 9);
                mBuffer2[index] = c;
            }
        }

        // Swap the buffer so that the blurred one is used
        Color32[] temp = mBuffer1;
        mBuffer1 = mBuffer2;
        mBuffer2 = temp;
    }

    /// <summary>
    /// 描绘一次性迷雾
    /// </summary>
    void RevealMap()
    {
        for (int y = 0; y < textureSize; ++y)
        {
            int yw = y * textureSize;

            for (int x = 0; x < textureSize; ++x)
            {
                int index = x + yw;
                Color32 c = mBuffer1[index];

                if (c.g < c.r)
                {
                    c.g = c.r;
                    mBuffer1[index] = c;
                }
            }
        }
    }

    /// <summary>
    /// Update the specified texture with the new color buffer.
    /// </summary>

    void UpdateTexture()
    {
        if (mScreenHeight != Screen.height || mTexture0 == null)
        {
            mScreenHeight = Screen.height;

            if (mTexture0 != null) Destroy(mTexture0);
            if (mTexture1 != null) Destroy(mTexture1);

            // Native ARGB format is the fastest as it involves no data conversion
            mTexture0 = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
            mTexture1 = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);

            mTexture0.wrapMode = TextureWrapMode.Clamp;
            mTexture1.wrapMode = TextureWrapMode.Clamp;

            mTexture0.SetPixels32(mBuffer0);
            mTexture0.Apply();
            mTexture1.SetPixels32(mBuffer1);
            mTexture1.Apply();
            mState = State.Blending;
        }
        else if (mState == State.UpdateTexture0)
        {
            // Texture updates are spread between two frames to make it even less noticeable when they get updated
            mTexture0.SetPixels32(mBuffer0);
            mTexture0.Apply();
            mState = State.UpdateTexture1;
            mBlendFactor = 0f;
        }
        else if (mState == State.UpdateTexture1)
        {
            mTexture1.SetPixels32(mBuffer1);
            mTexture1.Apply();
            mState = State.Blending;
        }
    }

    /// <summary>
    /// Checks to see if the specified position is currently visible.
    /// </summary>

    public bool IsVisible(Vector3 pos)
    {
        if (mBuffer0 == null) return false;
        pos -= mOrigin;

        float worldToTex = (float)textureSize / worldSize;
        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, textureSize - 1);
        cy = Mathf.Clamp(cy, 0, textureSize - 1);
        int index = cx + cy * textureSize;
        return mBuffer1[index].r > 96 || mBuffer0[index].r > 96;//默认0，改高要求
    }

    /// <summary>
    /// Checks to see if the specified position has been explored.
    /// </summary>

    public bool IsExplored(Vector3 pos)
    {
        if (mBuffer0 == null) return false;
        pos -= mOrigin;

        float worldToTex = (float)textureSize / worldSize;
        int cx = Mathf.RoundToInt(pos.x * worldToTex);
        int cy = Mathf.RoundToInt(pos.z * worldToTex);

        cx = Mathf.Clamp(cx, 0, textureSize - 1);
        cy = Mathf.Clamp(cy, 0, textureSize - 1);
        return mBuffer0[cx + cy * textureSize].g > 0;
    }
}