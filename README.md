# Bolsa de Trabajo Gubernamental

Solución .NET 8 para modernizar la contratación de servidores públicos: los agentes de selección publican plazas, los candidatos se registran, completan su hoja de vida y se postulan, y el proceso se gestiona desde la publicación hasta la contratación.

## Arquitectura (N-Capas)

| Proyecto | Responsabilidad |
|---|---|
| `BolsaTrabajo.Common` | Constantes de roles, `ResultadoOperacion<T>`, configuración de archivos. |
| `BolsaTrabajo.Entities` | Modelos (`Usuario`, `Plaza`, `Postulacion`, `HojaDeVida`), enums de estado y DTOs con validaciones. |
| `BolsaTrabajo.DAL` | `ApplicationDbContext` (IdentityDbContext), repositorios con EF Core, migraciones y datos iniciales. |
| `BolsaTrabajo.BL` | Servicios con las reglas de negocio y AutoMapper. |
| `BolsaTrabajo.API` | API REST con controladores, ASP.NET Core Identity (token Bearer), roles y Swagger. |
| `BolsaTrabajo.MVC` | Aplicación web MVC que consume la API (cookie + token), con vistas por rol. |
| `BolsaTrabajo.Tests` | Pruebas unitarias xUnit con base de datos InMemory. |

Flujo: `MVC  ->  API (Controllers)  ->  BL (Services)  ->  DAL (Repositories)  ->  SQL Server`

## Roles

- **Administrador**: gestiona usuarios y roles; monitorea todas las plazas y postulaciones.
- **Agente**: publica, edita, elimina y cambia el estado de sus plazas (Abierta, En evaluación, Cerrada); evalúa postulaciones (En revisión, Aprobado, Rechazado).
- **Candidato**: se registra, actualiza su perfil, gestiona su hoja de vida (con CV en PDF) y se postula a plazas.

## Ejecutar en Visual Studio 2022

1. Abrir `BolsaTrabajo.sln`.
2. Revisar la cadena de conexión `DefaultConnection` en `BolsaTrabajo.API/appsettings.json`. Por defecto usa LocalDB:
   `Server=(localdb)\mssqllocaldb;Database=BolsaTrabajoDB;Trusted_Connection=True;TrustServerCertificate=True`
   (si usa una instancia de SQL Server, cambie solo el `Server=`).
3. Presionar **F5**:
   - API (Swagger): https://localhost:7001/swagger
   - MVC: https://localhost:7002
   La MVC busca la API en `ApiSettings:BaseUrl` (`BolsaTrabajo.MVC/appsettings.json`).

Si el navegador muestra un aviso del certificado de desarrollo: `dotnet dev-certs https --trust`.

### Usuarios de demostración

Se crean automáticamente al iniciar la API (solo para pruebas):

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@bolsatrabajo.gob.sv | Admin123* |
| Agente | agente@bolsatrabajo.gob.sv | Agente123* |
| Candidato | candidato@bolsatrabajo.gob.sv | Candidato123* |

Nuevos candidatos pueden registrarse desde la MVC (**Registrarse**).

### Probar la API en Swagger

1. `POST /api/auth/login` con `{ "email": "...", "password": "..." }` y copiar el `accessToken`.
2. Botón **Authorize** e ingresar el token.

## Endpoints principales

| Método | Ruta | Rol |
|---|---|---|
| POST | `/api/auth/login` | Público (Identity) |
| POST | `/api/Usuarios/registro` | Público (crea Candidato) |
| GET/PUT | `/api/Usuarios/perfil` | Autenticado |
| GET/POST | `/api/Usuarios` | Administrador |
| PUT | `/api/Usuarios/{id}/rol` | Administrador |
| DELETE | `/api/Usuarios/{id}` | Administrador |
| GET | `/api/Usuarios/roles` | Administrador |
| GET | `/api/Plazas/disponibles`, `/api/Plazas/{id}` | Público |
| GET | `/api/Plazas` | Administrador |
| GET | `/api/Plazas/mis-plazas` | Agente |
| POST | `/api/Plazas` | Agente |
| PUT/DELETE | `/api/Plazas/{id}`, PUT `/api/Plazas/{id}/estado` | Agente dueño o Administrador |
| POST | `/api/Postulaciones` | Candidato |
| GET | `/api/Postulaciones/mis-postulaciones` | Candidato |
| GET | `/api/Postulaciones/plaza/{idPlaza}` | Agente dueño o Administrador |
| PUT | `/api/Postulaciones/{id}/estado` | Agente dueño o Administrador |
| GET | `/api/Postulaciones?estado=` | Administrador |
| GET/PUT | `/api/HojasDeVida/mia` | Candidato |
| POST | `/api/HojasDeVida/mia/archivo` | Candidato (PDF, máx. 5 MB) |
| GET | `/api/HojasDeVida/usuario/{idUsuario}` | Agente o Administrador |
| GET | `/api/HojasDeVida/{id}/archivo` | Dueño, Agente o Administrador |

## Reglas de negocio

- La fecha de publicación no puede ser pasada y la fecha de cierre debe ser posterior a la de publicación.
- Solo el agente que publicó la plaza (o un administrador) puede modificarla o evaluar sus postulaciones.
- Para postularse se requiere: usuario autenticado, plaza Abierta y vigente, hoja de vida registrada y no haberse postulado antes (también hay un índice único `IdPlaza + IdCandidato` en la base de datos).
- Al cerrar una plaza, las postulaciones que siguen "En revisión" pasan a "Rechazado".
- Una plaza con postulaciones no se elimina; se cierra para conservar el historial.
- El CV debe ser un PDF real (se verifica la extensión y la firma `%PDF`).

## Pruebas unitarias

Ejecutar desde el **Explorador de pruebas** de Visual Studio o con `dotnet test`. Las pruebas pedidas en las indicaciones son:

