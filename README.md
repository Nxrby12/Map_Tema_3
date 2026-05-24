# Map_Tema_3

Aplicatie desktop WPF (MVVM + Data Binding) pentru management restaurant cu comenzi online.

## Structura pe straturi
- `RestaurantApp.Domain` - entitati de domeniu
- `RestaurantApp.Application` - logica aplicatiei (meniu, cautare, comenzi, rapoarte)
- `RestaurantApp.Infrastructure` - repository in-memory (seed de date)
- `RestaurantApp.Presentation` - interfata WPF (MVVM)
- `Database/schema_and_procedures.sql` - schema SQL Server (3NF) + proceduri stocate

## Configurare
Valorile pentru discounturi, praguri si transport sunt in:
- `RestaurantApp.Presentation/appsettings.json`

## Testare build
```bash
dotnet restore MapTema3.slnx
dotnet build MapTema3.slnx -c Release
```

## Date demo autentificare
- Client: `client@example.com` / `client123`
- Angajat: `employee@example.com` / `employee123`
