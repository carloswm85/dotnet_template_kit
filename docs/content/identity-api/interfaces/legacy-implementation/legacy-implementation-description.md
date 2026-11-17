- [Data Contract Classes](#data-contract-classes)
  - [`ExchangeDataCompany`](#exchangedatacompany)
  - [`ExchangeData`](#exchangedata)
  - [`Permisos`](#permisos)
  - [`Sectores`](#sectores)
  - [`Usuarios`](#usuarios)
  - [Service Methods (`WebServiceAccesSoap`)](#service-methods-webserviceaccessoap)
    - [Synchronous Methods](#synchronous-methods)
    - [Asynchronous Methods](#asynchronous-methods)

---

# Data Contract Classes

- Old implementation used in a legacy .NET Framework 4.7.x project
- Auto-generated WCF service reference code and extract the methods and properties.

## `ExchangeDataCompany`

**Properties:**

- IdUsuario (int)
- Usuario (string)
- IdEmpresa (string)
- CodigoEmpresa (string)
- Empresa (string)

## `ExchangeData`

**Properties:**

- IdSesion (long)
- IdUsuario (int)
- Usuario (string)
- Nombre (string)
- Apellido (string)
- Password (string)
- Estado (string)
- Sector (string)
- Empresa (string)
- FechaInicio (DateTime)
- FechaFin (DateTime)
- Email (string)
- Telefono (string)
- Imagen (byte[])

## `Permisos`

**Properties:**

- IdPermiso (int)
- IdModulo (int)
- IdTipoPermiso (int)
- Baja (bool)

## `Sectores`

**Properties:**

- IdSector (int)
- Descripcion (string)
- Baja (bool)

## `Usuarios`

**Properties:**

- IdUsuario (int)
- Usuario (string)
- Password (string)
- Mail (string)
- Nombre (string)
- Apellido (string)
- Tel (string)
- IdSector (int)
- IdEmpresa (string)
- CambiarPassword (bool)
- OlvidoPassword (bool)
- Baja (bool)
- Imagen (byte[])

## Service Methods (`WebServiceAccesSoap`)

### Synchronous Methods

1. **ExchangeDataCompanyGetByIdUsuario**(int idUsuario) → ExchangeDataCompany[]
2. **obtainUserDataBySession**(int idSesion) → ExchangeData
3. **obtainUserDataBySession2**(int idSesion) → ExchangeData
4. **obtainModulosPermisosBySession**(int idSesion, int idSistema) → Permisos[]
5. **ModificarPassword**(int idUsuario, string oldPassword, string newPassword) → bool
6. **GuardarUsuario**(int idUsuario, string Nombre, string Apellido, string Mail, string Tel, int IdSector, string IdEmpresa) → bool
7. **SectoresGetAll**() → Sectores[]
8. **GuardarImagen**(int idUsuario, byte[] imagen) → bool
9. **UsuariosGetAll**() → Usuarios[]
10. **UsuariosSistemasPermisosAdd**(int idUser) → int

### Asynchronous Methods

All methods above have corresponding `Async` versions returning `Task<T>`
