public class HeroAI : AIManager
{
    public HeroAI(Actor theOwner) : base(theOwner)
    {
    }

    protected override void GetAIDataByType()
    {
        AIType = "hero1";
        ThinkInterval = 100;
    }
}
