using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillingPokerScrip : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using UnityEngine;

// 카드 문양
    public enum Suit
        {
            Hearts,   // ♥️ 하트
            Diamonds, // ♦️ 다이아몬드  
            Clubs,    // ♣️ 클럽
            Spades    // ♠️ 스페이드
        }

        // 카드 랭크
        public enum Rank
        {
            Ace = 1,
            Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
            Jack = 11, Queen = 12, King = 13
        }

        [System.Serializable]
        public class Card
        {
            public Suit suit;
            public Rank rank;

            public Card(Suit suit, Rank rank)
            {
                this.suit = suit;
                this.rank = rank;
            }

            // 카드의 기본 값 (1-13)
            public int GetBaseValue()
            {
                return (int)rank;
            }

            // 두 카드 조합의 특수 값 계산 (AA = 100점 룰 적용)
            public static int GetCombinationValue(Card card1, Card card2)
            {
                // AA 페어는 100점
                if (card1.rank == Rank.Ace && card2.rank == Rank.Ace)
                {
                    return 100;
                }

                // 일반적인 경우 두 카드의 합
                return card1.GetBaseValue() + card2.GetBaseValue();
            }

            // 카드 정보를 문자열로 반환
            public override string ToString()
            {
                string suitSymbol = suit switch
                {
                    Suit.Hearts => "♥️",
                    Suit.Diamonds => "♦️",
                    Suit.Clubs => "♣️",
                    Suit.Spades => "♠️",
                    _ => ""
                };

                string rankString = rank switch
                {
                    Rank.Ace => "A",
                    Rank.Jack => "J",
                    Rank.Queen => "Q",
                    Rank.King => "K",
                    _ => ((int)rank).ToString()
                };

                return $"{rankString}{suitSymbol}";
            }

            // 카드 비교 (정렬용)
            public static bool operator >(Card card1, Card card2)
            {
                return card1.GetBaseValue() > card2.GetBaseValue();
            }

            public static bool operator <(Card card1, Card card2)
            {
                return card1.GetBaseValue() < card2.GetBaseValue();
            }
        }

        public class Deck : MonoBehaviour
        {
            [SerializeField] private List<Card> cards = new List<Card>();

            void Start()
            {
                InitializeDeck();
                Shuffle();
            }

            // 52장 표준 덱 초기화
            public void InitializeDeck()
            {
                cards.Clear();

                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    {
                        cards.Add(new Card(suit, rank));
                    }
                }

                Debug.Log($"덱 초기화 완료: {cards.Count}장");
            }

            // 카드 셔플 (Fisher-Yates 알고리즘)
            public void Shuffle()
            {
                for (int i = cards.Count - 1; i > 0; i--)
                {
                    int randomIndex = UnityEngine.Random.Range(0, i + 1);
                    Card temp = cards[i];
                    cards[i] = cards[randomIndex];
                    cards[randomIndex] = temp;
                }

                Debug.Log("덱 셔플 완료");
            }

            // 카드 한 장 뽑기
            public Card DrawCard()
            {
                if (cards.Count == 0)
                {
                    Debug.LogWarning("덱에 카드가 없습니다!");
                    return null;
                }

                Card drawnCard = cards[0];
                cards.RemoveAt(0);
                return drawnCard;
            }

            // 여러 장 뽑기
            public List<Card> DrawCards(int count)
            {
                List<Card> drawnCards = new List<Card>();

                for (int i = 0; i < count && cards.Count > 0; i++)
                {
                    drawnCards.Add(DrawCard());
                }

                return drawnCards;
            }

            // 남은 카드 수
            public int RemainingCards()
            {
                return cards.Count;
            }

            // 덱 리셋 (게임 재시작용)
            public void ResetDeck()
            {
                InitializeDeck();
                Shuffle();
            }

            // 특정 카드가 덱에 있는지 확인 (디버그용)
            public bool Contains(Card card)
            {
                return cards.Any(c => c.suit == card.suit && c.rank == card.rank);
            }
        }

        // 조합 판정을 위한 유틸리티 클래스
        public static class CardCombination
        {
            // 연속된 숫자 판정 (보너스 +3)
            public static bool IsSequential(Card card1, Card card2)
            {
                int val1 = card1.GetBaseValue();
                int val2 = card2.GetBaseValue();

                // A-K, K-A도 연속으로 처리
                if ((val1 == 1 && val2 == 13) || (val1 == 13 && val2 == 1))
                    return true;

                return Math.Abs(val1 - val2) == 1;
            }

            // 같은 문양 판정 (보너스 +2)
            public static bool IsSameSuit(Card card1, Card card2)
            {
                return card1.suit == card2.suit;
            }

            // 같은 숫자 판정 (보너스 +2)
            public static bool IsSameRank(Card card1, Card card2)
            {
                return card1.rank == card2.rank;
            }

            // 홀수/짝수 통일 판정 (보너스 +1)
            public static bool IsAllEvenOrOdd(List<Card> cards)
            {
                if (cards.Count == 0) return false;

                bool isFirstEven = cards[0].GetBaseValue() % 2 == 0;
                return cards.All(card => (card.GetBaseValue() % 2 == 0) == isFirstEven);
            }

            // 조합 보너스 계산
            public static int GetBonusBullets(Card card1, Card card2, List<Card> allPlayerCards = null)
            {
                int bonus = 0;

                if (IsSequential(card1, card2)) bonus += 3;
                if (IsSameSuit(card1, card2)) bonus += 2;
                if (IsSameRank(card1, card2)) bonus += 2;

                // 전체 핸드가 홀수/짝수 통일되어 있으면 +1
                if (allPlayerCards != null && IsAllEvenOrOdd(allPlayerCards))
                    bonus += 1;

                return bonus;
            }
        }

        // 동점 처리를 위한 비교 클래스
        public static class TieBreaker
        {
            // 동점 시 우선순위: 페어 > 연속 > 같은 문양 > 좌석번호
            public static int CompareTwoCardHands(Card card1_A, Card card1_B, Card card2_A, Card card2_B, int seatNumber1, int seatNumber2)
            {
                // 1. 페어 확인
                bool isPair1 = CardCombination.IsSameRank(card1_A, card1_B);
                bool isPair2 = CardCombination.IsSameRank(card2_A, card2_B);

                if (isPair1 && !isPair2) return 1;  // 플레이어1 승리
                if (!isPair1 && isPair2) return -1; // 플레이어2 승리

                // 2. 연속 확인
                bool isSeq1 = CardCombination.IsSequential(card1_A, card1_B);
                bool isSeq2 = CardCombination.IsSequential(card2_A, card2_B);

                if (isSeq1 && !isSeq2) return 1;
                if (!isSeq1 && isSeq2) return -1;

                // 3. 같은 문양 확인
                bool isSuit1 = CardCombination.IsSameSuit(card1_A, card1_B);
                bool isSuit2 = CardCombination.IsSameSuit(card2_A, card2_B);

                if (isSuit1 && !isSuit2) return 1;
                if (!isSuit1 && isSuit2) return -1;

                // 4. 좌석번호로 최종 결정 (낮은 번호가 승리)
                if (seatNumber1 < seatNumber2) return 1;
                if (seatNumber1 > seatNumber2) return -1;

                return 0; // 완전 동점 (일어날 수 없음)
            }
        }
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
