# Dokumentacja Projektu MyWay

Projekt **MyWay** to zaawansowana aplikacja okienkowa (desktopowa) służąca do zarządzania produktywnością, nawykami oraz do śledzenia nastroju i celów. Aplikacja została zbudowana przy użyciu technologii **WPF (Windows Presentation Foundation)** w języku C# oraz wzorca architektonicznego **MVVM (Model-View-ViewModel)**. Za utrwalanie danych odpowiada lokalna baza **SQLite**.

Poniżej znajduje się dokładna dokumentacja kodu podzielona na poszczególne pliki i przestrzenie nazw.

---

## 1. Katalog `Models` (Modele Domenowe)

Modele definiują strukturę danych aplikacji. Dziedziczą one po klasie `ObservableObject` z biblioteki `CommunityToolkit.Mvvm`, co automatycznie zapewnia powiadamianie widoków o zmianach właściwości (implementacja `INotifyPropertyChanged`).

### `DailyRecord.cs`
Reprezentuje dzienny wpis użytkownika, zawierający statystyki z danego dnia.
* **Ważne zmienne i właściwości:**
  * `_id`, `_date`: Unikalny identyfikator wpisu oraz data, której dotyczy.
  * `_moodScore`: Ocena nastroju w skali 1-5 (domyślnie 3). Opatrzona atrybutami wymuszającymi aktualizację powiązanych właściwości UI po jej zmianie.
  * `_extraPoints`, `_taskPoints`: Punkty zdobyte dodatkowo oraz z realizacji zadań.
  * `_quoteOfTheDay`: Wylosowany na dany dzień cytat motywacyjny.
* **Funkcje (Właściwości wyliczane):**
  * `TotalPoints`: Oblicza całkowitą sumę punktów z danego dnia (`TaskPoints + ExtraPoints`).
  * `MoodLabel`: Zamienia wartość `MoodScore` (1-5) na opis słowny (np. "Świetnie", "Fatalnie").
  * `MoodEmoji`: Zwraca odpowiednią emotikonę na podstawie wartości `MoodScore`.

### `Habit.cs`
Przechowuje informacje o śledzonym nawyku.
* **Ważne zmienne i właściwości:**
  * `_title`: Nazwa/tytuł nawyku.
  * `_currentStreak`, `_bestStreak`: Obecna oraz rekordowa seria dni (streak) nieprzerwanego wykonywania nawyku.
  * `_lastCompletedDate`: Data ostatniego oznaczenia nawyku jako "wykonany".
  * `_isCompletedToday`: Flaga informująca, czy nawyk został dzisiaj ukończony.
* **Funkcje i właściwości wyliczane:**
  * `IsStreakBroken`: Sprawdza logicznie, czy od ostatniego ukończenia minęło więcej niż jeden pełny dzień (co oznacza przerwanie serii).
  * `StreakLabel`: Zwraca ładnie sformatowany tekst pokazujący obecną serię (np. "3 dni 🔥").
  * `CheckAndResetStreak()`: Funkcja sprawdzająca czy passa została przerwana; jeśli tak, resetuje `CurrentStreak` do zera oraz aktualizuje flagę `IsCompletedToday`.

### `TaskItem.cs`
Reprezentuje pojedyncze zadanie do wykonania w systemie.
* **Ważne zmienne i właściwości:**
  * `_title`, `_difficulty`: Nazwa zadania oraz jego poziom trudności (1-Łatwe, 2-Średnie, 3-Trudne).
  * `_dueDate`: Termin wykonania zadania.
  * `_isCompleted`, `_isOverdue`: Status zadania informujący o ukończeniu lub przekroczeniu terminu.
  * `_timeSpentSeconds`: Zsumowany czas w sekundach spędzony nad zadaniem (mierzony przez tryb "Skupienia").
* **Funkcje i właściwości wyliczane:**
  * `Points`: Bezpośrednio mapuje się na trudność (trudność jest ilością punktów do zdobycia).
  * `DifficultyLabel` / `DifficultyIcon`: Translacja numerycznej trudności na etykiety tekstowe i gwiazdki.
  * `TimeSpentLabel`: Przelicza sekundy na sformatowany ciąg czasu (np. `1h 5m 30s`).
  * `UpdateOverdue()`: Logika weryfikująca: jeśli zadanie nie jest ukończone i termin jest przeszły, ustawia flagę `IsOverdue` na `true`.

### `UserProfile.cs`
Zawiera globalne informacje o profilu zalogowanego gracza/użytkownika.
* **Ważne zmienne i właściwości:**
  * `_displayName`, `_avatarEmoji`: Wyświetlana nazwa i ikona profilu.
  * `_dailyPointsGoal`: Indywidualny cel punktowy wyznaczony na każdy dzień.
  * `_joinedDate`: Data dołączenia (rozpoczęcia korzystania z aplikacji).
* **Funkcje (Właściwości wyliczane):**
  * `JoinedLabel`: Sformatowany tekst z datą dołączenia.
  * `DaysActive`: Wylicza ilość dni korzystania z aplikacji.

---

## 2. Katalog `ViewModels` (Modele Widoków)

Pliki z warstwy ViewModel sterują logiką całej aplikacji. Oddzielają widoki od bazy danych.

### `RelayCommand.cs`
Definiuje klasy realizujące interfejs `ICommand`, umożliwiając bindowanie zdarzeń z XAML (np. kliknięcia w przycisk) do metod w ViewModelu.
* **Klasy:**
  * `RelayCommand`: Do synchronicznych akcji. `CanExecute` weryfikuje czy komenda może być uruchomiona, a `Execute` ją odpala.
  * `AsyncRelayCommand`: Odpowiednik dla akcji asynchronicznych (używa wewnątrz bloku `try-finally` i chroni przed podwójnym kliknięciem przez wewnętrzną flagę `_isExecuting`).

### `MainViewModel_updated.cs` (lub `MainViewModel.cs`)
Główny model agregujący wszystkie podrzędne ViewModele i odpowiadający za nawigację.
* **Ważne zmienne i wstrzykiwanie:**
  * Posiada instancje `TasksViewModel`, `DashboardViewModel`, `ProfileViewModel`.
  * W konstruktorze tworzy obiekty serwisów (`DatabaseService`, `QuoteService`, `PdfExportService`) i wstrzykuje je głębiej.
* **Logika:** W konstruktorze znajduje się powiązanie zdarzeń (Wire up) - subskrybuje zdarzenie ukończenia zadania `Tasks.PointsEarned`, by natychmiast asynchronicznie dodać punkty w `Dashboard` i odświeżyć statystyki `Profile`.

### `DashboardViewModel.cs`
Zarządza widokiem głównym (podsumowaniem, nawykami i nastrojem).
* **Ważne zmienne:**
  * Kolekcje: `ObservableCollection<Habit> Habits`, `PeriodRecords` (dane do statystyk).
  * Dane formularzy: `NewHabitTitle`, `StatsPeriod`.
* **Ważne funkcje / Logika:**
  * `LoadTodayAsync()`: Odpytuje bazę o wpis z dzisiejszego dnia. Jeśli brak – generuje nowy i losuje cytat na dany dzień.
  * `SaveMoodAsync()`: Wykonuje walidację (zakres 1-5) i aktualizuje ocenę nastroju na dany dzień.
  * `AddHabitAsync()`: Waliduje poprawność tytułu i duplikaty. Zapisuje nowy nawyk w bazie i odświeża kolekcję widoczną w UI.
  * `ToggleHabitAsync()`: Logika zaznaczania nawyku - jeśli zostaje oznaczony jako wykonany, powiększa "streak" (serię) i aktualizuje rekord. Jeśli odznaczony, zmniejsza.
  * `ExportPdfAsync()`: Komenda, która wykorzystuje `PdfExportService` do wyeksportowania bieżących statystyk nastroju.

### `TasksViewModel.cs`
Rozbudowany ViewModel odpowiedzialny za zadania, sortowanie, kalendarz oraz "Tryb Skupienia" (Stoper/Pomodoro).
* **Ważne zmienne:**
  * Filtry i sortowanie: `SelectedDate` (do kalendarza), `FilterText`, `FilterStatus`, `SortBy`.
  * `ICollectionView TasksView`: Wyciągnięty interfejs kolekcji pozwalający na łatwe nakładanie algorytmów filtrowania i sortowania.
  * Zmienne Stopera: `TimerRunning`, `TimerDisplay`, obiekt klasy `CancellationTokenSource`.
* **Ważne funkcje / Logika:**
  * `AddTaskAsync()`: Zapis zadania z walidacją – data w przeszłości nie przejdzie, tytuł nie może być pusty.
  * `FilterTask(object obj)`: Skomplikowana logika filtrująca dla `ICollectionView`. Decyduje, czy zadanie wyświetlić na liście (bada frazę tekstową, status i dopasowanie do wybranej w kalendarzu daty).
  * `ApplySort()`: Odświeża `SortDescriptions` w kolekcji, aktualizując widok względem rosnących/malejących dat lub trudności.
  * `ToggleTimer()`, `RunTimerAsync()`: Odpowiada za Tryb Skupienia. Działa asynchronicznie w tle (`Task.Delay`), odliczając sekundy dopóki nie zostanie wywołane anulowanie z poziomu `CancellationToken`. Następnie `SaveTimerAsync` dopisuje zebrany czas bezpośrednio do bazy.

### `ProfileViewModel.cs`
Zarządza danymi użytkownika i edycją profilu.
* **Zmienne i właściwości wyliczane:**
  * `Profile`: Przechowuje obiekt modelu logowania/celów.
  * `DailyProgress` / `DailyProgressLabel`: Zwracają procentowy oraz liczbowy postęp w realizacji dziennego celu punktowego (aby zasilić ewentualne paski postępu).
* **Funkcje:**
  * `BeginEdit()`, `CancelEdit()`, `SaveProfileAsync()`: Klasyczny zestaw akcji do włączania trybu edycji, wycofywania zmian i utrwalania zmodyfikowanego awatara/celu.

---

## 3. Katalog `Services` (Usługi i logika biznesowa)

Separacja logiki dostępu do danych oraz funkcji zewnętrznych od modeli.

### `DatabaseService.cs`
Centralny serwis obsługujący wszystkie zapytania i zapisy do lokalnej bazy danych SQLite.
* **Inicjalizacja:** Konstruktor domyślny tworzy plik `myway.db` w katalogu `%AppData%/MyWay` (używając struktury ścieżek `Environment.SpecialFolder.ApplicationData`) i wywołuje polecenia `CREATE TABLE IF NOT EXISTS` dla wszystkich modeli.
* **Funkcje dla encji (CRUD):**
  * `GetTasksAsync()`, `AddTaskAsync()`, `UpdateTaskAsync()`, `DeleteTaskAsync()` - Klasyczny zestaw CRUD do zarządzania zadaniami w sposób w pełni asynchroniczny z parametryzacją zapytań (np. wartości `cmd.Parameters.AddWithValue("$title", task.Title)` dla ochrony przed SQL Injection).
  * Posiada analogiczny zestaw CRUD dla Nawyków (Habits).
* **Funkcje zaawansowane:**
  * `UpsertDailyRecordAsync()`: Wykorzystuje klauzulę `ON CONFLICT(Date) DO UPDATE SET`, dzięki czemu jeśli istnieje wpis dla dzisiejszego dnia to zostaje on zaktualizowany; a jeżeli nie to utworzony – minimalizuje ilość kroków zapytań z bazy danych.

### `PdfExportService.cs`
Używa pakietu `PdfSharp`.
* **Funkcje:**
  * `ExportStatsAsync()`: Przyjmuje listę rekordów i używając mechanizmów rysowania wektorowego (`XGraphics`) generuje pełnostronicowy, odpowiednio sformatowany plik PDF podsumowujący statystyki. Plik jest zapisywany na Pulpicie (`Desktop`). Zapakowane w funkcję `Task.Run` dla uniknięcia zamrożenia głównego wątku (UI) podczas rysowania.

### `QuoteService.cs`
Prosty serwis do generowania lub dostarczania dziennych cytatów.
* **Logika:** Zamiast używać zewnętrznego i podatnego na awarię API internetowego, posiada wbudowaną statyczną kolekcję z potężnymi cytatami motywacyjnymi. Generuje odpowiedni indeks z uwzględnieniem "dnia roku", co zapewnia deterministyczny cytat (zmieniający się punktualnie o północy, stały na cały dzień).

---

## 4. Katalog `Converters` (Konwertery UI)

W katalogu zawarto definicje obiektów `IValueConverter`, które modyfikują w locie to co wyświetlane w XAML-u:
* `BoolToVisibilityConverter`: Tłumaczy `true/false` w C# na stany `Visible/Collapsed` z WPF.
* `DifficultyToColorConverter` i `MoodToColorConverter`: W zależności od poziomu int (`diff` / `mood`), zwracają instancję `SolidColorBrush` o określonym hex-kolorze (odpowiednio zielony/niebieski/różowy).
* `CompletedToStrikethroughConverter`: Zwraca efekt "przekreślenia tekstu", jeśli flaga zadania `IsCompleted` równa się logicznemu true.
* `StreakToColorConverter`: Moduluje kolor zależnie od "serii" (streak) – staje się on gorący (czerwony) po osiągnięciu określonej ilości sukcesów pod rząd.
* `ProgressWidthConverter` (`IMultiValueConverter`): Przyjmuje postęp wyrażony we ułamkach (0.0 do 1.0) oraz matematyczną szerokość "kontenera" w WPF, przemnażając je zwraca ostateczną wylotową grubość, która pozwala płynnie animować lub wyświetlać customowy Data-Grid/ProgressBar.

---

## 5. Katalog `Views` oraz pliki XAML

WPF opiera się na kodzie XAML definiującym układ (Layout).

### `Styles.xaml`
* Słownik zasobów, który został zaincludowany w `App.xaml`, aby stał się widoczny z każdego okna w aplikacji.
* Definiuje pełen "System Design" (Design System):
  * Dynamiczne pędzle (Brushes): `AccentPurpleBrush`, `BgDarkBrush`.
  * Szablony kontrolek (ControlTemplates): Zdefiniowane na nowo przyciski `BaseButton`, zaokrąglone w nowym stylu `DarkTextBox`, customowe pole Combo (`FilterComboBox`), ukrywające standardowe archaiczne kontrolki Windows'a.

### `MainWindow_updated.xaml` (lub `MainWindow.xaml`)
* Główne okno typu "Shell". Okno posiada zmodyfikowaną ramkę przez `WindowChrome`, aby zaimplementować całkowicie własny estetyczny systemowy pasek tytułu.
* Podział na pasek boczny (Sidebar) po lewej stronie do nawigacji oraz główną, renderowaną przestrzeń po prawej. Wykorzystuje właściwość `Visibility` zarządzaną z View-Models.

### Inne kontrolki: `TasksView.xaml` / `DashboardView.xaml`
* Layout złożony oparty w przeważającej mierze o tagi `<Grid>` oraz `<StackPanel>`.
* Użyto potężnych tagów `<ListView>` do dynamicznego renderowania w wierszach oraz szablonowania przez `<DataTemplate>`.
* W `TasksView.xaml` mamy Modal Overlay dla "Trybu skupienia", który w XAML zachowuje Z-Index ponad normalnym Gridem i wyciemnia obszar roboczy, jeśli uaktywniony jest parametr (połączony przez Konwerter do zmiennej logicznej).

---

## 6. Realizacja Kryteriów Oceny Projektu

Aplikacja z nawiązką spełnia określone wymogi, o czym świadczą poniższe odniesienia w kodzie.

### A. Wymagania minimalne (MVP) - 50%
1. **CRUD na co najmniej jednej encji:**
   Aplikacja posiada pełny cykl życia dla encji `TaskItems` oraz `Habits`. Dodawanie (`AddTaskAsync`), usuwanie (`DeleteTaskAsync`), odczyt list z możliwościami sortowania (`GetTasksAsync`), edycja oraz aktualizacja (`UpdateTaskAsync`).
2. **Trwałość danych:**
   Wprowadzono profesjonalną lokalną bazę bazy `SQLite`. Plik z danymi tworzy się w specjalnym bezpiecznym katalogu z użyciem `Environment.SpecialFolder.ApplicationData`. Działa on po restarcie komputera i jest nieulotny.
3. **Podstawowa logika biznesowa:**
   * Reguła 1: Obliczenie upływu dni w seriach/nawykach i przerywanie jej (streak), jeśli różnica czasu pomiędzy zapisami w bazie przekracza 1 dzień (`Habit.IsStreakBroken`).
   * Reguła 2: Dodanie nowego zadania uniemożliwia zapisanie daty mniejszej niż "dziś" (twardo weryfikowane w logice `TasksViewModel`).
4. **Obsługa podstawowych błędów użytkownika i Walidacja wejścia z feedbackiem:**
   Zastosowano w polach modelu widoku walidację. Przykładowo formularze wyświetlają czytelny błąd dzięki zmiennym `TitleError` oraz `DueDateError` (feedback). Jeżeli użytkownik spróbuje zatwierdzić formularz z naruszeniem biznesowej reguły wykonanie metody zatrzyma się z czytelnym `MessageBox.Show()`.
5. **UI zbudowane w XAML, z sensownym layoutem:**
   Aplikacja cechuje się wysoce estetycznym wyglądem. Posiada starannie dobrane kolory oraz padding/margin oparty w całości o zaawansowane kontrolki layoutu (`Grid`, `StackPanel`, `Border`). Zagnieżdżenia są logiczne i podzielone na UserControls.
6. **Wykorzystanie Data Bindingu:**
   Zrealizowano na setki sposobów. Od Data Bindingu obiektowego dla kolekcji list za pomocą `ItemsSource="{Binding TasksView}"`, po precyzyjny Data Binding wartości wejściowych m.in formularzy tekstowych `Text="{Binding NewTitle, UpdateSourceTrigger=PropertyChanged}"`.
7. **Wykorzystanie Commands:**
   Logika akcji wykorzystuje autorską implementację klas implementujących `ICommand` (tzw. `RelayCommand` oraz jej mutację `AsyncRelayCommand`). Ponad 20 oddzielnych komend (np. `Command="{Binding DeleteTaskCommand}"`) izoluje zachowanie XAML (np. przycisków) od logiki w Code-Behind.
8. **Poprawny kod:**
   Brak błędów krytycznych, czysty kod. W pełni wykorzystano wzorzec wstrzykiwania zależności oraz asynchroniczność.

### B. Rozwinięcia i jakość (Punkty Dodatkowe)

* **Rozbudowa domeny i funkcjonalności (do 20%):**
  * *Druga encja i relacje*: System ma cztery rozbudowane osobne tabele w bazie (TaskItems, Habits, DailyRecords, UserProfile). Oddziałują one na siebie logicznie – zdobyte punkty z zadań automatycznie zasilają wpisy w statystykach.
  * *Wielokryteriowe filtrowanie i sortowanie*: Klasa `TasksViewModel` posiada niesamowicie mocną logikę odpytywania z zastosowaniem `ICollectionView` – pozwala na jednoczesne filtrowanie po nazwie, dacie względem kalendarza oraz przefiltrowanie widoku według 4 rodzajów sortowań.
  * *PDF / Wydruk*: Pomyślnie zaimplementowano bibliotekę `PdfSharp` i stworzono serwis `PdfExportService`, generujący elegancki wektorowy raport nastroju oraz statystyk jako PDF zapisywany prosto na pulpit.

* **Zaawansowane mechanizmy WPF (do 10%):**
  * *Style i słowniki zasobów*: Wszystkie style wydzielono do ogromnego słownika `Styles.xaml`.
  * *Szablony danych/kontrolek i Triggery*: W tym słowniku zdefiniowano szablony `ControlTemplate` (m.in dla Custom Combobox'a i CheckBox'ów WPFowych) a także DataTriggery dla ożywienia UI za pomocą najechania myszki (`IsMouseOver`) czy statusu ukończenia zadania (`IsCompleted`). Użyto mnóstwo autorskich Konwerterów rzutujących na kolor tekstu lub widoczność.

* **Architektura i dobre praktyki (do 10%):**
  * Solidna architektura SQLite.
  * Podręcznikowy wzorzec MVVM wraz z wstrzykiwaniem zależności (Dependency Injection przez konstruktor w `MainViewModel`).
  * Wszechobecna *Asynchroniczność* – każdy element zapisu, bazy danych lub generacji PDFu realizowany jest w modelu `async/await` zapobiegającym tzw. mrożeniu okna Windowsowego (UI lock).

* **Innowacyjność (do 10%):**
  * Stworzono innowacyjny "Tryb skupienia" (Stoper pomodoro do pojedynczego zadania). Moduł ten oparty jest na zaawansowanym modelu przerywania wątku poprzez obiekty `CancellationTokenSource`. Wykorzystując overlay w XAML, tworzy pełny profesjonalny widget, który zlicza asynchronicznie dedykowany czas i automatycznie aktualizuje encję w bazie po powrocie z pracy. To wykracza poza standardowy moduł CRUD.
