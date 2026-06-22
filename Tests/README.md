# Unit tests

xUnit-tests voor de MAUI-onafhankelijke logica van de app.

De hoofd-app is een .NET MAUI-project en heeft de MAUI-workload nodig om te
bouwen. Om de tests overal (lokaal en in CI) te kunnen draaien, compileert dit
testproject de specifieke broncode onder test rechtstreeks mee (zie de
`<Compile Include="..\..." />`-regels in `bioscoop-app.Tests.csproj`). Alle
ingesloten bestanden gebruiken alleen `System.*` en de eigen modellen — geen
MAUI-types.

## Draaien

```bash
dotnet test Tests/bioscoop-app.Tests.csproj
```

## Wat wordt getest

- **`RecommendationServiceTests`** — de aanbevelingslogica: leeg resultaat zonder
  films, algemene selectie zonder favorieten, favorieten worden nooit
  aanbevolen, ranking op genre-overlap (case-insensitief en getrimd) en het
  respecteren van de `max`-limiet.
- **`ReservationModelTests`** — de berekende properties van een reservering:
  `IsCancelled`, `IsUpcoming`, `IsCheckedIn`, `SeatsSummary`, `HallDisplay` en
  het stoel-`Label`.
