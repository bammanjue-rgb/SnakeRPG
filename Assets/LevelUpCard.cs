using UnityEngine;

public enum CardType
{
    AttackPlus1, AttackPlus2, AttackPlus3,
    MaxHPPlus1, MaxHPPlus2, MaxHPPlus3,
    HealPlus1, HealPlus2, HealPlus3
}

[System.Serializable]
public class LevelUpCard
{
    public CardType type;
    public string title;
    public string description;
    public int probability;
    
    public static LevelUpCard[] GetRandomCards(int count)
    {
        LevelUpCard[] allCards = new LevelUpCard[]
        {
            new LevelUpCard { type = CardType.AttackPlus1, title = "공격력 +1", description = "공격력이 1 증가합니다", probability = 10 },
            new LevelUpCard { type = CardType.AttackPlus2, title = "공격력 +2", description = "공격력이 2 증가합니다", probability = 5 },
            new LevelUpCard { type = CardType.AttackPlus3, title = "공격력 +3", description = "공격력이 3 증가합니다", probability = 3 },
            new LevelUpCard { type = CardType.MaxHPPlus1, title = "체력 최대치 +1", description = "최대 체력이 1 증가합니다", probability = 10 },
            new LevelUpCard { type = CardType.MaxHPPlus2, title = "체력 최대치 +2", description = "최대 체력이 2 증가합니다", probability = 5 },
            new LevelUpCard { type = CardType.MaxHPPlus3, title = "체력 최대치 +3", description = "최대 체력이 3 증가합니다", probability = 3 },
            new LevelUpCard { type = CardType.HealPlus1, title = "체력 회복 +1", description = "체력을 1 회복합니다", probability = 10 },
            new LevelUpCard { type = CardType.HealPlus2, title = "체력 회복 +2", description = "체력을 2 회복합니다", probability = 5 },
            new LevelUpCard { type = CardType.HealPlus3, title = "체력 회복 +3", description = "체력을 3 회복합니다", probability = 3 }
        };
        
        LevelUpCard[] selectedCards = new LevelUpCard[count];
        System.Collections.Generic.List<LevelUpCard> cardPool = new System.Collections.Generic.List<LevelUpCard>(allCards);
        
        for (int i = 0; i < count && cardPool.Count > 0; i++)
        {
            int totalWeight = 0;
            foreach (var card in cardPool) totalWeight += card.probability;
            
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var card in cardPool)
            {
                currentWeight += card.probability;
                if (randomValue < currentWeight)
                {
                    selectedCards[i] = card;
                    cardPool.Remove(card);
                    break;
                }
            }
        }
        return selectedCards;
    }
}