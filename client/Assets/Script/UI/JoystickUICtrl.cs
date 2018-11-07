/* 
	UI View Gen From GenUITools
	Please Don't Modify!
 */

using UnityEngine;
using UnityEngine.UI;

public class JoystickUICtrl : MonoBehaviour
{
    public Image sprBase;
    public Image sprTouch;

	public void Awake()
	{
        sprBase = transform.Find("widget/sprBase").GetComponent<Image>();
        sprTouch = transform.Find("widget/sprTouch").GetComponent<Image>();

	}
}