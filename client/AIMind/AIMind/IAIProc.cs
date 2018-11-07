namespace AIMind
{
    public interface IAIProc
    {
        #region 条件接口
        bool CheckSelf();
        bool CheckRandom(int random);
        bool FindAttackTarget();
        bool FindFollowTarget();
        bool CheckAttackRange();
        bool TooFarFollowTarget();
        #endregion

        #region 行为接口
        bool MoveToPoint(float x, float z, float range);
        bool StopMoveTo();
        bool StareBlanklyInThinkCount(int thinkCount);
        bool MoveByForwardInThinkCount(int thinkCount);
        bool TurnToRandomDir(float angle1, float angle2);
        bool ChangeAction(string actStr);
        bool MoveToAttackTarget();
        bool MoveToFollowTarget();
        bool AttackTheTarget();
        #endregion
    }
}