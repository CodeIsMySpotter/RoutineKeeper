# RoutineKeeper - Ustalenia Projektowe i Wytyczne

Ten plik zawiera wszystkie kluczowe decyzje architektoniczne i wizualne dla projektu **RoutineKeeper**. Został wygenerowany, aby każdy Agent pracujący nad tym projektem w przyszłości znał kontekst i trzymał się ustalonych ram.

## 1. Technologia i Architektura
- **Platforma**: .NET MAUI (C#).
- **Wzorzec**: MVVM z wykorzystaniem `CommunityToolkit.Mvvm`.
- **Baza Danych**: Ściśle lokalna – **SQLite** (`sqlite-net-pcl`). Dane użytkownika zostają na urządzeniu (brak backendu zsynchronizowanego w chmurze na ten moment dla podstawowych danych).
- **Powiadomienia**: Aplikacja korzysta z natywnych, systemowych powiadomień mobilnych wraz z **alarmami dźwiękowymi**, które skutecznie przypomną o zmianie zadania. (np. `Plugin.LocalNotification`).

## 2. Główne Funkcjonalności
- **Planner (Widok tygodnia)**: Zarządzanie blokami czasowymi (Uczelnia, Praca, Siłownia, Gotowanie, Odpoczynek).
- **Dashboard (Kalkulator Odpoczynku)**: Inteligentne obliczanie wolnego czasu (24h minus zaplanowane zadania i sen), dające użytkownikowi poczucie kontroli i relaksu.
- **Aktywne Timery**: Widok odliczający czas do końca bieżącego bloku.

## 3. Asystent AI (Faza późniejsza)
- Projekt zakłada zaimplementowanie w przyszłości inteligentnego asystenta (np. w oparciu o Google Auth do weryfikacji dostępu).
- Użytkownik będzie mógł prowadzić tekstową konwersację z Agentem wewnątrz aplikacji.
- Agent zinterpretuje potrzeby użytkownika i wygeneruje **strukturę JSON**, którą aplikacja zdeserializuje i automatycznie wrzuci jako nowe bloki do lokalnej bazy SQLite.

## 4. Wygląd i Design System (UI/UX)
- Aplikacja ma mieć na celu **uspokojenie użytkownika** i redukcję stresu w trakcie wymagającego semestru.
- **Główny motyw**: Ciemny (tzw. GitHub Dark).
  - Tło podstawowe: `#0D1117`
  - Kontenery / Karty: `#161B22`
  - Obramowania (Border): `#30363D`
- **Kolor Akcentu**: Kojąca, zgaszona zieleń (np. delikatny szmaragd, mięta). Unikamy jaskrawych, agresywnych neonów. Zieleń reprezentuje tutaj ukojenie, relaks i podsumowania czasu wolnego.
- **Typografia**: Nowoczesna, czysta czcionka bezszeryfowa. Liczniki i timery powinny być duże i wyraźne.
