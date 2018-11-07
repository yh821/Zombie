public class MonstorAI : AIManager
{
    public MonstorAI(Actor theOwner) : base(theOwner)
    {
    }

    protected override void GetAIDataByType()
    {
        AIType = "zombie1";
        ThinkInterval = 200;
    }
}
