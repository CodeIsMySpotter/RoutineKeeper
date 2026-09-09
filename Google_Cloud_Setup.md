# Jak skonfigurować Google Cloud pod RoutineKeeper (Logowanie OAuth2)

Aby logowanie działało w aplikacji desktopowej na Windows, musisz wygenerować **Client ID** w konsoli Google Cloud i ustawić odpowiedni Redirect URI. 

Postępuj zgodnie z poniższymi instrukcjami krok po kroku:

### Krok 1: Utworzenie projektu i ekranu zgody
1. Wejdź na stronę [Google Cloud Console](https://console.cloud.google.com/).
2. Utwórz nowy projekt (przycisk w lewym górnym rogu obok logo Google Cloud) i nazwij go np. "RoutineKeeper".
3. Z lewego menu wybierz **APIs & Services (Interfejsy API i usługi)** -> **OAuth consent screen (Ekran logowania OAuth)**.
4. Wybierz typ użytkownika **External (Zewnętrzny)** i kliknij *Create*.
5. Wypełnij wymagane pola (App name: "RoutineKeeper", User support email: podaj swój email, Developer contact information: podaj swój email). Resztę zostaw pustą.
6. Przejdź dalej. W sekcji **Scopes** kliknij *Add or remove scopes* i zaznacz:
   - `.../auth/userinfo.email`
   - `.../auth/userinfo.profile`
   - `openid`
7. Przejdź do sekcji **Test users** i dodaj swój adres e-mail (dopóki aplikacja ma status "Testing", tylko zapisani tu użytkownicy mogą się zalogować).
8. Kliknij *Save and Continue* aż do końca.

### Krok 2: Utworzenie danych uwierzytelniających (Client ID)
1. W lewym menu kliknij **Credentials (Dane logowania)**.
2. Na górze kliknij **+ CREATE CREDENTIALS** -> **OAuth client ID**.
3. **BARDZO WAŻNE:** W polu *Application type* (Typ aplikacji) wybierz **iOS** (Tak, iOS! Aplikacje desktopowe MAUI na Windows pod spodem korzystają ze standardu podobnego do aplikacji mobilnych, a Google wycofało stare typy desktopowe. Dla custom URL scheme używa się typu iOS).
4. W polu *Name* wpisz np. "RoutineKeeper Windows".
5. W polu **Bundle ID** wpisz: `com.companyname.routinekeeper`
6. Kliknij **Create (Utwórz)**.

> **UWAGA:** W najnowszych wersjach Google Cloud dla MAUI Windows (WinUI3) używa się typu "iOS" (lub czasem "Android"), by uzyskać pełne wsparcie dla niestandardowego schematu URI. Jeśli wybierzesz "Desktop", Google pozwoli tylko na `http://127.0.0.1`, co jest trudne do obsłużenia w MAUI na Windows. Z typem iOS dostaniesz Client ID, który pozwala na redirect pod Twój custom URI.
> 
> *Schemat niestandardowy to odwrotność nazwy domeny Twojego Client ID. Będzie wyglądał podobnie do `com.googleusercontent.apps.123456789-abcdef`*.

### Krok 3: Aktualizacja aplikacji

Po utworzeniu otrzymasz **Client ID** (np. `123456789-abcdef.apps.googleusercontent.com`).

Skopiuj go i wklej do aplikacji używając polecenia (w terminalu Windows):
```bash
setx GoogleAuth__ClientId "TWÓJ_WYGENEROWANY_CLIENT_ID"
```

Oraz musisz podać swój **Redirect URI** w kodzie/zmiennej.
Redirect URI tworzy się poprzez **odwrócenie Client ID** i dodanie `:/oauth2redirect`.
Np. jeśli Twój Client ID to `123456789-abcdef.apps.googleusercontent.com`, to Twój Redirect URI wynosi:
`com.googleusercontent.apps.123456789-abcdef:/oauth2redirect`

Będziesz musiał ustawić go w MAUI. Zrobię to za Ciebie w kodzie tak, by wystarczyło go zdefiniować. Ustalmy, że po prostu użyjemy schematu `routinekeeper://auth` jako naszego uniwersalnego rozwiązania. Jeśli Google tego nie przyjmie z typu iOS, użyjemy `http://localhost:5000` (typ Desktop). Wdrażam pierwszą standardową opcję.
