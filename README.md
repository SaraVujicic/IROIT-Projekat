##Sistem za Upravljanje Odsustvima

Ovaj projekat predstavlja mikroservisnu arhitekturu za upravljanje odsustvima zaposlenih (Absence, Leave Request, Leave Balance, Employee, Department, Notification) razvijenu u **.NET 8.0** za backend i **Vite + React** za frontend.

U okviru ove nadogradnje, u sistem su integrisani:
1. **GitHub Actions CI/CD Pipeline** za automatsko testiranje, statičku analizu i deployment na **Azure Container Apps**.
2. **Statička analiza** pomoću alata **SonarCloud** sa uključenom proverom **Quality Gate-a**.
3. **Asinhrona komunikacija** zasnovana na **RabbitMQ** brokeru sa produkcionim karakteristikama (durable queues, persistent messages, manual ACKs, DLQ/DLX i automatic connection recovery).
4. **Strukturisano logovanje** korišćenjem biblioteke **Serilog** i jedinstvenog formata sa propagacijom **Correlation ID-a** i lokalnog **Request ID-a**.
5. **Observability & Monitoring**:
   - **Prometheus** za prikupljanje HTTP i sistemskih metrika (latency, throughput, error rates, active requests).
   - **Grafana** sa pre-konfigurisanim dashboard-om za vizuelizaciju rada sistema i RabbitMQ statusa.
   - **OpenTelemetry & Jaeger** za distribuirano praćenje (distributed tracing) kroz sve mikroservise.
   - **Health Checks** endpoint-i na svakom servisu.

---

## 1. Kako pokrenuti kompletan sistem lokalno (Docker Compose)

Celu infrastrukturu i aplikativne servise možete pokrenuti jednom komandom iz direktorijuma `backend`:

```bash
# Pokretanje svih servisa u pozadini
docker compose -f backend/docker-compose.yml up -d
```

Ova komanda pokreće sledeće kontejnere:
- **Baze podataka**: 6 odvojenih PostgreSQL instanci (jedna za svaki mikroservis)
- **Komunikacioni broker**: RabbitMQ sa uključenim management plugin-om
- **Monitoring**: Prometheus, Grafana i Jaeger (Distributed Tracing)
- **Aplikativni servisi**: ApiGateway i 6 funkcionalnih mikroservisa (svi kontejneri imaju definisan automatski EF Core migracioni korak na startu, restart policy i healthcheck provere).

Za gašenje celog sistema i uklanjanje volumena koristite:
```bash
docker compose -f backend/docker-compose.yml down -v
```

---

## 2. Pristup Alatima i Dashboard-ima

Nakon uspešnog pokretanja sistema, sledeće usluge su dostupne lokalno:

| Usluga | URL | Podrazumevani kredencijali | Opis |
| --- | --- | --- | --- |
| **API Gateway** | `http://localhost:8080` | - | Ulazna tačka za sve API pozive |
| **Swagger UI (Request)** | `http://localhost:8080/swagger` | - | Dokumentacija i testiranje API-ja |
| **RabbitMQ Management** | `http://localhost:15672` | `guest` / `guest` | Upravljanje porukama i redovima |
| **Prometheus** | `http://localhost:9090` | - | Pregled prikupljenih metrika sistema |
| **Grafana** | `http://localhost:3000` | `admin` / `admin` | Vizuelni dashboard sa metrikama |
| **Jaeger UI** | `http://localhost:16686` | - | Pregled distribuiranih trace-ova |
| **Health Check (Gateway)** | `http://localhost:8080/health` | - | Status zdravlja API Gateway-a |

---

## 3. Asinhrona Komunikacija (RabbitMQ)

Komunikacija između mikroservisa se odvija asinhrono u sledećim slučajevima:
1. **Kreiranje zahteva**: Kada korisnik podnese zahtev preko `RequestService`, generiše se `RequestCreatedEvent` i šalje na RabbitMQ exchange `request-events-exchange` sa routing key-om `request.created`.
2. **Promena statusa**: Kada menadžer ili admin odobri/odbije zahtev u `RequestService`, generiše se `RequestStatusChangedEvent` i šalje sa routing key-om `request.statuschanged`.

### Produkciona Robusnost:
- **Durable & Persistent**: Redovi (`request-events-queue`) i exchanges su deklarisani kao durable, a same poruke se šalju kao persistent (`DeliveryMode = 2`) kako ne bi došlo do gubitka podataka u slučaju pada brokera.
- **Manual ACK**: Potrošač (`NotificationService`) šalje ručnu potvrdu (ACK) brokeru tek kada uspešno zapiše notifikaciju u svoju bazu podataka.
- **Dead Letter Queue (DLQ)**: Ukoliko obrada poruke u `NotificationService` ne uspe (npr. zbog greške u bazi podataka), poruka se odbija (NACK sa `requeue: false`) i RabbitMQ je automatski preusmerava u Dead Letter Exchange (`request-events-dlx`) i Dead Letter Queue (`request-events-dlq`).
- **Connection Recovery & Retry**: Obe strane imaju implementiran retry mehanizam sa eksponencijalnim kašnjenjem prilikom povezivanja i automatski oporavak konekcije (`AutomaticRecoveryEnabled`).

---

## 4. Monitoring i Observability

### Metrike (Prometheus & Grafana)
Svaki mikroservis izlaže standardne metrike na `/metrics` endpoint-u preko biblioteke `prometheus-net.AspNetCore`.
Grafana ima automatski učitan Prometheus data source i dashboard pod nazivom **"Microservices Observability Dashboard"** koji vizuelizuje:
- **Throughput** (broj HTTP zahteva u sekundi).
- **Average HTTP Latency** (srednje vreme odgovora servisa).
- **Error Rate** (procenat neuspešnih 4xx i 5xx odgovora).
- **Active Requests** (broj trenutno aktivnih konekcija).
- **Resource Usage** (CPU i memorijsko opterećenje po kontejneru).
- **RabbitMQ Status**: broj poruka u redu, broj slobodnih/ready poruka i broj aktivnih potrošača.

### Distribuirani Trace-ovi (OpenTelemetry & Jaeger)
Svaki HTTP zahtev koji uđe u sistem dobija jedinstveni **Correlation ID** (generiše se na gateway-u ili prvom servisu i propagira kroz zaglavlje `X-Correlation-ID` u downstream pozivima).
Preko **OpenTelemetry** SDK-a, svi servisi šalju trace podatke na Jaeger gRPC endpoint (`http://jaeger:4317`). Otvaranjem Jaeger dashboard-a na `http://localhost:16686` možete videti kompletan put zahteva (npr. od `ApiGateway` preko `RequestService` do eksternog poziva ka `EmployeeService`).

---

## 5. Strukturisano Logovanje (Serilog)

Svi servisi koriste **Serilog** biblioteku koja obezbeđuje uniforman format logova ispisanih na konzolu. Logovi sadrže:
- `{CorrelationId}` - jedinstveni ID koji povezuje sve logove generisane tokom obrade jednog korisničkog zahteva kroz različite servise.
- `{RequestId}` - lokalni Request ID za lakše debagovanje pojedinačnog API poziva na nivou tog servisa.
- `{Application}` - naziv mikroservisa koji je upisao log.

---

## 6. GitHub Actions CI/CD Pipeline

Konfiguracioni fajlovi se nalaze u `.github/workflows/`:
- **CI Pipeline (`ci.yml`)**: Pokreće se na push i pull request na bilo kojoj grani. Podiže privremenu bazu podataka i RabbitMQ u Dockeru, instalira pakete, build-uje backend i frontend, pokreće testove, lintere, izvršava SonarCloud analizu i proverava **Quality Gate**. Nakon testiranja gasi kontejnere.
- **CD Pipeline (`cd.yml`)**: Pokreće se prilikom merge-ovanja PR-a u `main` granu. Build-uje Docker slike za sve mikroservise, push-uje ih na **Azure Container Registry (ACR)** i vrši automatski update na **Azure Container Apps**.

### Podešavanje GitHub Secrets
Da bi CI/CD radio, potrebno je na GitHub repozitorijumu dodati sledeće secret-e:
- `AZURE_CREDENTIALS` - JSON sa Service Principal kredencijalima:
  ```json
  {
    "clientId": "<GUID>",
    "clientSecret": "<STRING>",
    "subscriptionId": "<GUID>",
    "tenantId": "<GUID>"
  }
  ```
- `AZURE_REGISTRY_SERVER` - ACR server (npr. `mojregistar.azurecr.io`).
- `AZURE_REGISTRY_USERNAME` - ACR korisničko ime.
- `AZURE_REGISTRY_PASSWORD` - ACR lozinka.
- `SONAR_TOKEN` - Token generisan na SonarCloud nalogu za autentifikaciju.

---

## 7. Lokalne Komande za Testiranje i Razvoj

Ukoliko želite ručno da pokrenete provere lokalno pre nego što push-ujete kod na GitHub:

```bash
# 1. Build celog backend rešenja
dotnet build backend/AbsenceService/Absence.slnx
dotnet build backend/DepartmentService/DepartmentService.sln
dotnet build backend/EmployeeService/EmployeeManagement.sln
dotnet build backend/LeaveBalanceService/LeaveBalanceService.sln
dotnet build backend/NotificationService/NotificationService.sln
dotnet build backend/RequestService/RequestService.sln
dotnet build backend/ApiGateway/ApiGateway.csproj

# 2. Pokretanje svih backend testova
dotnet test backend/AbsenceService/Absence.slnx
dotnet test backend/DepartmentService/DepartmentService.sln
dotnet test backend/EmployeeService/EmployeeManagement.sln
dotnet test backend/LeaveBalanceService/LeaveBalanceService.sln
dotnet test backend/NotificationService/NotificationService.sln
dotnet test backend/RequestService/RequestService.sln

# 3. Instalacija i pokretanje lintera na frontend-u
cd frontend
npm ci
npm run lint
npm run build
```
