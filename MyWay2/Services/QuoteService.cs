using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWay.Services
{
    public class QuoteService
    {
        // Offline fallback quotes (PL) — used when no internet
        private static readonly List<string> _fallbackQuotes = new()
        {
            "Każdy wielki sen zaczyna się od śniącego. — Harriet Tubman",
            "Nie liczy się szybkość, liczy się kierunek. — Konfucjusz",
            "Sukces to suma małych wysiłków powtarzanych dzień po dniu. — Robert Collier",
            "Jedynym sposobem na wielką pracę jest kochać to co się robi. — Steve Jobs",
            "Jutro nigdy nie nadchodzi. Działaj dziś. — Napoleon Hill",
            "Nie bój się powolnego postępu. Bój się braku postępu. — przysłowie chińskie",
            "Możesz stracić majątek, ale nie możesz stracić nawyków. — anon.",
            "Dyscyplina to wolność wyboru. — Jocko Willink",
            "Mała poprawa każdego dnia daje wielkie efekty z czasem. — James Clear",
            "Zrób dziś to, czego nie chce robić przeciętny człowiek. — Earl Nightingale",
            "Twoje nawyki dziś to Twoje wyniki jutro. — anon.",
            "Motywacja Cię uruchamia. Nawyk Cię prowadzi. — Jim Ryun",
            "Człowiek staje się tym, o czym myśli przez cały dzień. — Ralph Waldo Emerson",
            "Droga do sukcesu i droga do porażki są prawie identyczne. — Colin R. Davis",
            "Zrób jeden krok, a droga pojawi się sama. — anon.",
            "Konsekwencja bije talent, gdy talent nie jest konsekwentny. — anon.",
            "Cel bez planu to tylko marzenie. — Antoine de Saint-Exupéry",
            "Nieważne jak wolno idziesz, ważne że się nie zatrzymujesz. — Konfucjusz",
            "Twoja energia jest walutą — wydawaj ją mądrze. — anon.",
            "Każdy ekspert był kiedyś początkującym. — Helen Hayes"
        };

        public async Task<string> GetDailyQuoteAsync()
        {
            // Deterministic daily selection based on day of year
            var dayIndex = DateTime.Today.DayOfYear % _fallbackQuotes.Count;
            await Task.Delay(10); // simulate async
            return _fallbackQuotes[dayIndex];
        }

        public string GetRandomQuote()
        {
            var rng = new Random(DateTime.Today.DayOfYear);
            return _fallbackQuotes[rng.Next(_fallbackQuotes.Count)];
        }
    }
}
