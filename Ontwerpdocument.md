# Bioscoop-app (.NET MAUI)
## Ontwerpdocument

| | |
|---|---|
| **Naam** | _[vul je naam in]_ |
| **Studentnummer** | _[vul je studentnummer in]_ |
| **Opleiding** | AVD AD Informatica |
| **Onderwijsperiode** | OP 2.4 — Portfolio .NET MAUI |
| **Project** | Bioscoop-app + BackendAPI |

---

## Inhoudsopgave

1. [Inleiding](#1-inleiding)
2. [User Stories](#2-user-stories)
   - [Bezoeker](#bezoeker)
   - [Medewerker](#medewerker)
   - [Beheerder](#beheerder)
3. [Use case diagram](#3-use-case-diagram)
4. [Wireframes](#4-wireframes)
5. [Web API-documentatie](#5-web-api-documentatie)
6. [Authenticatie en autorisatie](#6-authenticatie-en-autorisatie)
7. [Package diagram](#7-package-diagram)
8. [Deployment diagram](#8-deployment-diagram)
9. [Klassendiagrammen](#9-klassendiagrammen)
10. [Sequence diagrammen](#10-sequence-diagrammen)
    - [Toelichting MVVM-structuur](#toelichting-mvvm-structuur)
    - [Interactie tussen de .NET MAUI-app en de backend](#interactie-tussen-de-net-maui-app-en-de-backend)

---

## 1. Inleiding

Dit project bestaat uit twee onderdelen die samen één bioscoop-platform vormen:

- **`bioscoop-app`** — een cross-platform **.NET MAUI**-app (Android, iOS, macOS, Windows)
  waarmee bezoekers films bekijken, kaartjes reserveren en betalen, en waarmee
  medewerkers tickets bij de ingang scannen.
- **`bioscoop-api`** — een **ASP.NET Core Web API** (`BackendAPI`) met een MySQL/SQL Server
  database via Entity Framework Core, plus een **Blazor**-beheerpaneel (`BioscoopFrontend`)
  voor het beheer van films, zalen, voorstellingen, tarieven en arrangementen.

De app is opgezet volgens het **MVVM-patroon** met `CommunityToolkit.Mvvm`, waarbij elke
View via dependency injection een ViewModel krijgt en elke ViewModel via een interface een
Service aanspreekt die het HTTP-verkeer met de backend afhandelt (zie hoofdstuk 10).

De User Stories worden iteratief opgepakt, vergelijkbaar met Scrum. Op die manier is er
altijd een werkende applicatie die steeds meer features krijgt; valt een feature uit
(bijvoorbeeld door een beperking in MAUI of in beschikbare tijd), dan blijft de rest van de
app gewoon functioneren.

---

## 2. User Stories

Voor de prioritering is de **MoSCoW**-methode gebruikt: **M**ust have, **S**hould have,
**C**ould have en **W**on't have. Alles wat geen *Must have* is, kan gezien worden als een
wens die toegevoegd wordt als daar tijd voor is.

Er is onderscheid gemaakt tussen drie rollen: de **bezoeker** (de klant die films bekijkt en
reserveert), de **medewerker** (die kaartjes scant aan de ingang) en de **beheerder** (die via
het beheerpaneel de catalogus onderhoudt). Deze rollen sluiten elkaar niet uit — een
medewerker kan ook gewoon bezoeker zijn.

### Bezoeker

| # | Als bezoeker wil ik… | …zodat… | Prioriteit |
|---|---|---|---|
| B1 | een overzicht zien van alle films die nu draaien | ik weet uit welke films ik kan kiezen | Must |
| B2 | de details van een film bekijken (beschrijving, duur, leeftijd, genre, poster) | ik een geïnformeerde keuze kan maken | Must |
| B3 | de voorstellingen (tijden en zalen) van een film zien | ik kan kiezen wanneer ik ga | Must |
| B4 | een stoel kiezen uit een visuele zaalplattegrond | ik op mijn favoriete plek kan zitten | Must |
| B5 | een tarief kiezen (bijv. volwassene, kind, student) | ik de juiste prijs betaal | Must |
| B6 | online betalen via een veilige betaalpagina (Stripe) | ik mijn reservering direct kan afronden | Must |
| B7 | een bevestiging met QR-code/printcode ontvangen | ik mijn kaartje bij de ingang kan tonen | Must |
| B8 | mijn reserveringen terugzien in de app | ik mijn bezoeken kan beheren | Must |
| B9 | inloggen/registreren via Auth0 | mijn reserveringen en voorkeuren bewaard blijven | Must |
| B10 | een film aan mijn favorieten toevoegen | ik die snel terugvind | Should |
| B11 | mijn reservering wijzigen (andere stoelen) of annuleren | ik me kan aanpassen aan veranderingen | Should |
| B12 | aanbevelingen krijgen op basis van mijn favorieten/genres | ik makkelijk nieuwe films ontdek | Should |
| B13 | arrangementen (popcorn, drank, snacks) bij mijn ticket bestellen | ik alles in één keer regel | Should |
| B14 | de bioscopen op een kaart zien | ik weet welke locatie het dichtst bij is | Should |
| B15 | een pushnotificatie krijgen ~30 minuten voor mijn voorstelling | ik op tijd vertrek | Could |
| B16 | een pushnotificatie krijgen als ik in de buurt van de bioscoop ben | ik herinnerd word aan mijn bezoek | Could |
| B17 | mijn notificatievoorkeuren instellen | ik alleen meldingen krijg die ik wil | Could |
| B18 | feedback/een beoordeling achterlaten | de bioscoop de service kan verbeteren | Could |
| B19 | me inschrijven voor de nieuwsbrief | ik op de hoogte blijf van het aanbod | Could |
| B20 | een "secret screening" ontdekken | ik verrast word met een onaangekondigde film | Won't (deze iteratie volledig) |

### Medewerker

| # | Als medewerker wil ik… | …zodat… | Prioriteit |
|---|---|---|---|
| M1 | de QR-/printcode van een ticket scannen met de camera | ik snel toegang kan verlenen aan de ingang | Must |
| M2 | direct zien of een ticket geldig is of al ingecheckt is | ik dubbel gebruik van een kaartje voorkom | Must |
| M3 | een reservering op printcode kunnen opzoeken | ik kan helpen als de QR niet scant | Should |

### Beheerder

> Uitgevoerd via het Blazor-beheerpaneel (`BioscoopFrontend`), niet via de MAUI-app.

| # | Als beheerder wil ik… | …zodat… | Prioriteit |
|---|---|---|---|
| A1 | films toevoegen, wijzigen en verwijderen | de catalogus actueel blijft | Must |
| A2 | voorstellingen (film + zaal + tijd) inplannen | bezoekers kaartjes kunnen kopen | Must |
| A3 | zalen en stoelindelingen beheren | de zaalplattegrond klopt | Must |
| A4 | tarieven beheren | de juiste prijzen gelden | Must |
| A5 | arrangementen beheren | het horeca-aanbod up-to-date is | Should |
| A6 | films importeren uit TMDb | ik snel een rijke catalogus opbouw | Should |
| A7 | nieuwsbrief-abonnees beheren en een mailing versturen | ik bezoekers kan informeren | Could |

> _Tip: verwerk deze User Stories in een scrumboard (bijv. YouTrack of Azure DevOps) en voeg
> hier een screenshot van het board toe als **Figuur 1**._

---

## 3. Use case diagram

```mermaid
flowchart LR
    Bezoeker(["👤 Bezoeker"])
    Medewerker(["👤 Medewerker"])
    Beheerder(["👤 Beheerder"])

    subgraph App["Bioscoop-app (.NET MAUI)"]
        UC1(["Films bekijken"])
        UC2(["Filmdetails bekijken"])
        UC3(["Voorstellingen bekijken"])
        UC4(["Stoel & tarief kiezen"])
        UC5(["Betalen (Stripe)"])
        UC6(["Reservering bekijken/wijzigen/annuleren"])
        UC7(["Favorieten beheren"])
        UC8(["Aanbevelingen krijgen"])
        UC9(["Notificaties ontvangen"])
        UC10(["Feedback geven"])
        UC11(["Inloggen (Auth0)"])
        UC12(["Ticket scannen / inchecken"])
    end

    subgraph Admin["Beheerpaneel (Blazor)"]
        UC20(["Films beheren"])
        UC21(["Voorstellingen beheren"])
        UC22(["Zalen & stoelen beheren"])
        UC23(["Tarieven beheren"])
        UC24(["Arrangementen beheren"])
        UC25(["TMDb-import"])
        UC26(["Nieuwsbrief beheren"])
    end

    Bezoeker --- UC1 & UC2 & UC3 & UC4 & UC5 & UC6 & UC7 & UC8 & UC9 & UC10 & UC11
    Medewerker --- UC11 & UC12
    Beheerder --- UC20 & UC21 & UC22 & UC23 & UC24 & UC25 & UC26

    UC4 -. include .-> UC3
    UC5 -. include .-> UC4
    UC6 -. extend .-> UC5
```

---

## 4. Wireframes

Hieronder staan de schermen van de app beschreven, gekoppeld aan de bijbehorende
`Views`. Vervang de beschrijvingen eventueel door echte screenshots/Figma-wireframes.

| Scherm (View) | Beschrijving |
|---|---|
| **`MainPage` / `HomeViewModel`** | Startscherm met aanbevelingen, uitgelichte films en aanbiedingen. Bevat de tabbalk (Home, Favorieten, Reserveringen, Kaart) en de Pathé-titelbalk met profielknop. |
| **`MoviesPage` / `MoviesViewModel`** | Overzicht van alle films in een `CollectionView` met poster, titel en genre. Pull-to-refresh laadt opnieuw. |
| **`MovieDetailPage` / `MovieDetailViewModel`** | Detailpagina: poster, beschrijving, duur, leeftijdskeuring, genre, favorietknop en knop naar de voorstellingen. |
| **`MovieScreenings` / `MovieScreeningsViewModel`** | Lijst met voorstellingen van de gekozen film (datum, tijd, zaal). |
| **`ScreeningPage` / `ScreeningViewModel`** | Visuele **zaalplattegrond**: rijen en stoelen, bezette stoelen geblokkeerd, geselecteerde stoelen gemarkeerd, met tariefkeuze per stoel. |
| **`PaymentPage` / `PaymentViewModel`** | Overzicht van gekozen stoelen, tarieven en optionele arrangementen met totaalbedrag. |
| **`CheckoutPage`** | Ingebouwde `WebView` die de Stripe-betaalpagina toont; success/cancel-redirects worden onderschept. |
| **`ConfirmationPage`** | Bevestiging met **QR-code/printcode** van de reservering. |
| **`ReservationsPage` / `ReservationsViewModel`** | Lijst met de reserveringen van de ingelogde gebruiker (op e-mail), met offline cache. |
| **`ReservationDetailPage` / `ReservationDetailViewModel`** | Detail van één reservering met QR-code, knoppen voor stoelen wijzigen en annuleren. |
| **`EditSeatsPage` / `EditSeatsViewModel`** | Andere stoelen kiezen voor een bestaande reservering. |
| **`FavoritesPage` / `FavoritesViewModel`** | Lijst met films die de gebruiker als favoriet heeft gemarkeerd. |
| **`MapPage`** | Kaart (`Microsoft.Maui.Controls.Maps`) met de bioscooplocaties. |
| **`NotificationPreferencesPage` / `NotificationPreferencesViewModel`** | Schakelaars voor "herinnering vóór voorstelling" en "melding bij aankomst". |
| **`FeedbackPage` / `FeedbackViewModel`** | Formulier voor een beoordeling (rating + categorie + bericht). |
| **`LoginPage` / `ProfilePage`** | Auth0-login en profielweergave (naam, e-mail, foto). |
| **`CheckInScannerPage` / `CheckInScannerViewModel`** | **Medewerkerscherm**: camera-barcodescanner (`BarcodeScanning.Native.Maui`) die de QR-code leest en de reservering incheckt. |

> _Voeg hier de echte wireframes toe als **Figuur 2 t/m n**, bijvoorbeeld:
> MainPage, MovieDetail, ScreeningPage (zaalplattegrond), PaymentPage en ConfirmationPage._

---

## 5. Web API-documentatie

De backend (`BackendAPI`) is gedocumenteerd met **Swagger/OpenAPI** (Swashbuckle). In
Development is de Swagger-UI bereikbaar. Hieronder een overzicht van alle endpoints,
inclusief de vereiste autorisatie.

> Legenda autorisatie: **Anon** = openbaar · **Auth** = ingelogde gebruiker (cookie) ·
> **Manager** = rol `Manager` vereist.

### Movies — `api/movies`
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| GET | `/` | Anon | Alle films |
| GET | `/{id}` | Anon | Eén film |
| GET | `/upcoming` | Anon | Binnenkort te verwachten films |
| GET | `/{id}/details` | Anon | Uitgebreide filmdetails |
| GET | `/secret-screening` | Anon | Verborgen "secret screening" |
| POST | `/` | Manager | Film toevoegen |
| PUT | `/{id}` | Manager | Film wijzigen |
| DELETE | `/{id}` | Manager | Film verwijderen |
| POST | `/import?pages=` | Manager | Films importeren uit TMDb |

### Screenings — `api/screenings`
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| GET | `/?movieId=` | Anon | Voorstellingen (optioneel per film) |
| GET | `/overview` | Anon | Overzicht van voorstellingen |
| GET | `/{id}` | Anon | Eén voorstelling |
| GET | `/available-seats?movieId=` | Anon | Beschikbare stoelen voor een film |
| GET | `/{id}/seats` | Anon | Stoelen + bezetting voor een voorstelling |
| POST | `/` | Manager | Voorstelling inplannen |
| PUT | `/{id}` | Manager | Voorstelling wijzigen |
| DELETE | `/{id}` | Manager | Voorstelling verwijderen |

### Halls — `api/halls`
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| GET | `/` · `/{id}` | Anon | Zalen ophalen |
| POST · PUT · DELETE | `/` · `/{id}` | Manager | Zaal beheren |

### Tariffs — `api/tariffs`
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| GET | `/` · `/{id}` | Anon | Tarieven ophalen |
| POST · PUT · DELETE | `/` · `/{id}` | Manager | Tarief beheren |

### Arrangements — `api/arrangements`
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| GET | `/?category=` | Anon | Actieve arrangementen (optioneel per categorie) |
| GET | `/categories` | Anon | Beschikbare categorieën |
| GET | `/{id}` | Anon | Eén arrangement |
| GET | `/all` | Manager | Alle arrangementen (incl. inactieve) |
| POST · PUT · DELETE | `/` · `/{id}` | Manager | Arrangement beheren |

### Reservations — `api/reservations`
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| POST | `/` | Auth | Reservering aanmaken (app) |
| POST | `/website` | Anon | Reservering via website |
| GET | `/by-code/{code}` | Auth | Reservering op printcode |
| GET | `/by-email/{email}` | Auth | Reserveringen van een gebruiker |
| PUT | `/{code}/seats` | Auth | Stoelen van een reservering wijzigen |
| POST | `/{code}/cancel` | Auth | Reservering annuleren |
| POST | `/{code}/checkin` | Auth | Ticket inchecken (medewerker) |

### Orders & Betaling
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| POST | `api/orders` | Auth | Bestelling aanmaken |
| POST | `api/orders/{id}/pay` | Auth | Betaling bevestigen |
| POST | `api/stripe/checkout` | Auth | Stripe Checkout-sessie starten |
| POST | `api/stripe/confirm` | Auth | Stripe-betaling bevestigen |

### Feedback & Nieuwsbrief & Auth
| Methode | Route | Autorisatie | Omschrijving |
|---|---|---|---|
| POST | `api/feedback` | Anon | Feedback/beoordeling indienen |
| POST | `api/newsletter/subscribe` | Anon | Inschrijven op nieuwsbrief |
| GET | `api/newsletter/subscribers` | Manager | Abonnees ophalen |
| DELETE | `api/newsletter/subscribers/{id}` | Manager | Abonnee verwijderen |
| POST | `api/newsletter/send` | Manager | Mailing versturen |
| POST | `api/auth/login` | Anon | Inloggen (beheerpaneel, cookie) |
| POST | `api/auth/logout` | Auth | Uitloggen |
| GET | `api/auth/me` | Auth | Huidige gebruiker |

> _Voeg hier een screenshot van de Swagger-UI toe als **Figuur n**._

---

## 6. Authenticatie en autorisatie

Het platform kent **twee gescheiden authenticatiemechanismen**, passend bij de twee
client-typen:

### 6.1 Mobiele app — Auth0 (OIDC)
De .NET MAUI-app gebruikt **Auth0** via `Auth0.OidcClient.MAUI`. De bezoeker logt in via de
systeembrowser (Authorization Code Flow met PKCE). Belangrijke punten:

- **Configuratie** (`MauiProgram.cs`): domain `dev-…eu.auth0.com`, redirect `myapp://callback/`,
  scope `openid profile email offline_access`.
- **Sessie-herstel**: bij het opstarten (`App.OnStart`) wordt met een opgeslagen
  **refresh token** stil opnieuw ingelogd (`RefreshTokenAsync`), zonder de browser te openen.
  De `IUserSession` bewaart het token veilig (SecureStorage) en houdt de `ClaimsPrincipal` bij.
- De gebruiker hoeft **niet** ingelogd te zijn om films en voorstellingen te bekijken. Een
  account is wél nodig voor reserveren, betalen en favorieten.

### 6.2 Beheerpaneel & API — cookie-authenticatie met rollen
De `BackendAPI` gebruikt **cookie-authenticatie** (`Program.cs`):

- Cookie `CinemaAuth`, `SameSite=None`, `Secure=Always`, sliding expiration van 8 uur.
- Inloggen via `POST api/auth/login`; het wachtwoord wordt geverifieerd met
  `PasswordHasher<UserModel>`. Bij succes wordt een cookie met claims (naam + **rol**) gezet.
- **Twee rollen**: `Manager` (volledige rechten op films, zalen, voorstellingen, tarieven,
  arrangementen en nieuwsbrief) en `Cashier` (kassa/medewerker). Beheer-endpoints zijn
  beveiligd met `[Authorize(Roles = "Manager")]`; lees-endpoints zijn `[AllowAnonymous]`.
- Bij een geweigerde toegang geeft de API netjes **401** of **403** terug in plaats van een
  redirect, zodat de SPA/clients het kunnen afhandelen.
- **CORS** staat alleen de bekende frontend-origins toe (`AllowCredentials`).

```mermaid
flowchart TD
    subgraph Mobiel["📱 .NET MAUI-app (bezoeker/medewerker)"]
        A1[Login via systeembrowser] --> A2[Auth0]
        A2 -->|id/access/refresh token| A3[IUserSession + SecureStorage]
        A3 -->|access token / e-mail| A4[Reservering & betaling]
    end
    subgraph Web["💻 Blazor-beheerpaneel (Manager/Cashier)"]
        B1[Login formulier] --> B2[POST api/auth/login]
        B2 -->|cookie CinemaAuth + rol-claim| B3[Beheer-endpoints]
        B3 -->|Authorize Roles=Manager| B4[(BackendAPI)]
    end
```

---

## 7. Package diagram

```mermaid
flowchart TB
    subgraph Client["bioscoop-app (.NET MAUI)"]
        Views["Views (XAML)"]
        ViewModels["ViewModels (CommunityToolkit.Mvvm)"]
        ServicesI["Services (interfaces)"]
        ServicesImpl["Services (HttpClient + JSON)"]
        ModelsC["Models (DTO's)"]
        Views --> ViewModels --> ServicesI
        ServicesImpl -. implementeert .-> ServicesI
        ViewModels --> ModelsC
        ServicesImpl --> ModelsC
    end

    subgraph Server["bioscoop-api"]
        Controllers["Controllers"]
        ServicesS["Services (ReservationService, TmdbService, Stripe, Email…)"]
        ModelsS["Models / DTO's"]
        EF["ApplicationDbContext (EF Core)"]
        Controllers --> ServicesS --> EF
        Controllers --> ModelsS
        EF --> ModelsS
    end

    subgraph Frontend["BioscoopFrontend (Blazor)"]
        Pages["Pages / AdminPanel"]
        FeServices["Services"]
        Pages --> FeServices
    end

    ServicesImpl -->|HTTPS / JSON| Controllers
    FeServices -->|HTTPS / cookie| Controllers
    ServicesS -->|extern| TMDb["TMDb API"]
    ServicesS -->|extern| Stripe["Stripe API"]
    ServicesS -->|extern| SMTP["SMTP (MailKit)"]
    EF --> DB[("MySQL / SQL Server")]
```

---

## 8. Deployment diagram

```mermaid
flowchart LR
    subgraph Devices["Apparaten"]
        Phone["📱 Android / iOS toestel<br/>bioscoop-app"]
        Desktop["💻 Windows / macOS<br/>bioscoop-app"]
        Browser["🌐 Browser<br/>Blazor beheerpaneel"]
    end

    subgraph AppServer["Applicatieserver (Kestrel / ASP.NET Core)"]
        API["BackendAPI<br/>(REST + Swagger)"]
        Blazor["BioscoopFrontend<br/>(Blazor)"]
    end

    DB[("Database<br/>MySQL (macOS) / SQL Server (Windows)")]
    Auth0["Auth0<br/>(OIDC identity provider)"]
    StripeSvc["Stripe<br/>(betalingen)"]
    TMDbSvc["TMDb<br/>(filmdata)"]
    Mail["SMTP-server<br/>(nieuwsbrief)"]

    Phone -->|HTTPS JSON| API
    Desktop -->|HTTPS JSON| API
    Browser -->|HTTPS cookie| Blazor --> API
    Phone -->|OIDC| Auth0
    API --> DB
    API --> StripeSvc
    API --> TMDbSvc
    API --> Mail
```

---

## 9. Klassendiagrammen

### 9.1 Domeinmodel (backend)

```mermaid
classDiagram
    class MovieModel {
        +Guid MovieId
        +string Title
        +string Description
        +int DurationMinutes
        +int Age
        +string Genre
        +string? ImageUrl
        +DateTimeOffset CreatedAtUtc
    }
    class ScreeningModel {
        +Guid ScreeningId
        +Guid MovieId
        +Guid HallId
        +DateTimeOffset StartTimeUtc
    }
    class HallModel {
        +Guid HallId
        +int Number
        +LayoutType LayoutType
    }
    class SeatModel {
        +Guid SeatId
        +Guid HallId
        +string RowLabel
        +int SeatNumber
    }
    class OrderModel {
        +Guid OrderId
        +Guid ScreeningId
        +Guid? SeatId
        +Guid? TariffId
        +string Status
        +string PaymentStatus
        +string PaymentMethod
        +decimal TotalAmount
        +string PrintCode
        +string? UserEmail
        +DateTimeOffset CreatedAtUtc
        +DateTimeOffset? PaidAtUtc
        +DateTimeOffset? CheckedInAtUtc
    }
    class TariffModel {
        +Guid TariffId
        +string TariffType
        +string DisplayName
        +decimal Price
        +int SortOrder
    }
    class ArrangementModel {
        +Guid ArrangementId
        +string Name
        +string? Description
        +ArrangementCategory Category
        +decimal Price
        +bool IsActive
    }
    class OrderArrangementModel {
        +Guid OrderArrangementId
        +Guid OrderId
        +Guid ArrangementId
        +int Quantity
        +decimal UnitPrice
        +decimal LineTotal
    }
    class UserModel {
        +int UserId
        +string Username
        +string PasswordHash
        +string Role
    }
    class FeedbackModel {
        +Guid Id
        +string? Email
        +int Rating
        +string? Category
        +string Message
        +DateTime CreatedAtUtc
    }
    class LayoutType {
        <<enum>>
        Unknown
        Standard
        Imax
        Vip
        Deluxe
        ThirdDimensional
    }
    class ArrangementCategory {
        <<enum>>
        Popcorn
        Drank
        Snack
        ComboPackage
        HorecaSpecial
    }

    MovieModel "1" --> "*" ScreeningModel : draait in
    HallModel "1" --> "*" ScreeningModel : huisvest
    HallModel "1" --> "*" SeatModel : bevat
    ScreeningModel "1" --> "*" OrderModel
    SeatModel "0..1" <-- "*" OrderModel
    TariffModel "0..1" <-- "*" OrderModel
    OrderModel "1" --> "*" OrderArrangementModel
    ArrangementModel "1" --> "*" OrderArrangementModel
    HallModel --> LayoutType
    ArrangementModel --> ArrangementCategory
```

### 9.2 Client-architectuur (MAUI, MVVM)

```mermaid
classDiagram
    direction LR
    class MoviesPage {
        <<View>>
        +OnAppearing()
    }
    class MoviesViewModel {
        <<ViewModel>>
        +ObservableCollection~MovieModel~ Movies
        +bool IsBusy
        +LoadCommand
    }
    class IMovieService {
        <<interface>>
        +GetMoviesAsync() Task~List~MovieModel~~
    }
    class MovieService {
        -HttpClient _http
        -JsonSerializerOptions _json
    }
    class MovieModel {
        +Guid Id
        +string Title
        +string? ImageUrl
    }

    MoviesPage --> MoviesViewModel : BindingContext
    MoviesViewModel --> IMovieService : depends on
    MovieService ..|> IMovieService
    MovieService --> MovieModel : deserialiseert
    MoviesViewModel --> MovieModel : exposeert
```

> Hetzelfde drielagenpatroon (View → ViewModel → `I…Service`) geldt voor alle features:
> `IScreeningService`, `IReservationService`, `IPaymentService`, `ITariffService`,
> `IFavoritesService`, `IOfferService`, `IRecommendationService`, `IFeedbackService`,
> `INotificationPreferencesService`, `INotificationScheduler` en `IProximityService`.

---

## 10. Sequence diagrammen

### Toelichting MVVM-structuur

De app volgt strikt het **MVVM-patroon** met `CommunityToolkit.Mvvm` en compiled bindings
(`x:DataType`). Elke laag kent alleen de laag eronder, en alleen via een **interface**:

- **View (XAML)** — uitsluitend declaratieve UI; bindt aan properties en commands van de
  ViewModel. De code-behind bevat alleen `InitializeComponent()`, het zetten van de
  `BindingContext` en eventueel `OnAppearing`.
- **ViewModel** — houdt de state (`[ObservableProperty]`) en commands (`[RelayCommand]`) bij,
  kent géén MAUI-types en doet zelf geen HTTP. Krijgt via DI een service-interface.
- **Service** — bezit de gedeelde `HttpClient` en de JSON-(de)serialisatie en praat met de
  backend. Geregistreerd als **singleton** (zodat er één `HttpClient` is); detail-/edit-VM's
  en hun pagina's zijn **transient**.
- **Model** — eenvoudige DTO-klassen die de JSON van de API spiegelen.

Dependency injection wordt centraal geconfigureerd in `MauiProgram.cs`; niets wordt met de
hand `new`'d. Dit maakt de ViewModels los testbaar (een nep-`IMovieService` met vaste data is
genoeg om `LoadAsync` te testen, zonder MAUI of backend).

```mermaid
sequenceDiagram
    actor U as Gebruiker
    participant V as MoviesPage (View)
    participant VM as MoviesViewModel
    participant S as MovieService (IMovieService)
    participant API as BackendAPI

    U->>V: opent Films-tab
    V->>VM: LoadCommand.ExecuteAsync()
    VM->>VM: IsBusy = true (spinner aan)
    VM->>S: GetMoviesAsync()
    S->>API: GET /api/movies
    API-->>S: 200 OK + JSON
    S-->>VM: List<MovieModel>
    VM->>VM: Movies.Clear() + Add(...)
    VM-->>V: CollectionChanged → CollectionView hertekent
    VM->>VM: IsBusy = false (spinner uit)
```

### Interactie tussen de .NET MAUI-app en de backend

Onderstaand sequence diagram toont de volledige **reserveer-en-betaal-flow**, het hart van
de interactie tussen app en backend, inclusief Stripe en de QR-bevestiging.

```mermaid
sequenceDiagram
    actor U as Bezoeker
    participant ScrV as ScreeningPage
    participant PayVM as PaymentViewModel
    participant ResS as ReservationService
    participant PayS as PaymentService
    participant Co as CheckoutPage (WebView)
    participant API as BackendAPI
    participant Stripe as Stripe

    U->>ScrV: kiest stoel(en) + tarief
    ScrV->>PayVM: navigeert met selectie
    U->>PayVM: bevestigt bestelling
    PayVM->>ResS: ReserveAsync(stoelen, tarieven, e-mail)
    ResS->>API: POST /api/reservations
    API-->>ResS: reservering + PrintCode
    PayVM->>PayS: CreateCheckoutSessionAsync()
    PayS->>API: POST /api/stripe/checkout
    API->>Stripe: maak Checkout Session
    Stripe-->>API: checkout-URL
    API-->>PayS: checkout-URL
    PayS-->>Co: open URL in WebView
    U->>Stripe: voert betaling uit
    Stripe-->>Co: redirect naar success-URL
    Co->>API: POST /api/stripe/confirm
    API-->>Co: betaling bevestigd
    Co-->>U: ConfirmationPage met QR-code (PrintCode)
```

En de **check-in-flow** aan de ingang door een medewerker:

```mermaid
sequenceDiagram
    actor M as Medewerker
    participant Sc as CheckInScannerPage
    participant VM as CheckInScannerViewModel
    participant RS as ReservationService
    participant API as BackendAPI

    M->>Sc: scant QR-code van ticket
    Sc->>VM: gedecodeerde PrintCode
    VM->>RS: GetByCodeAsync(code)
    RS->>API: GET /api/reservations/by-code/{code}
    API-->>VM: reservering (status)
    VM->>RS: CheckInAsync(code)
    RS->>API: POST /api/reservations/{code}/checkin
    API-->>VM: 200 OK (ingecheckt) / fout (al gebruikt)
    VM-->>M: groen vinkje of waarschuwing
```

---

_Einde ontwerpdocument._
