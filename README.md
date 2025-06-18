# 🌐 API XyzTest - PostgreSQL

Esta API está diseñada para ejecutarse en cualquier entorno compatible con .NET, siempre que se configuren correctamente las variables de entorno y exista una base de datos PostgreSQL activa.

## 📋 Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) (versión Utilizada: `9.0`)
- PostgreSQL instalado y con una base de datos creada (versión Utilizada: `17`)

---

## 🗃️ Configuración de Base de Datos

Antes de iniciar la API, debes tener una base de datos PostgreSQL creada manualmente. Puedes utilizar el nombre, usuario y contraseña que desees, pero estos deben coincidir con las variables de entorno que configurarás.

### 📌 Datos mínimos que debes definir:

- **Host** (ej: `localhost`)
- **Nombre de la base de datos**
- **Usuario**
- **Contraseña**

### 🛢️ Archivo con Base de datos de ejemplo:
- En texto plano : **[BaseDeDatos.txt](https://github.com/user-attachments/files/20789874/dbData.txt)**
- Archivo SQL : **[BaseDeDatos.sql](https://drive.google.com/file/d/1rPJCRo1LcPMDbBcrb_O28LbSaTTyjBFH/view?usp=sharing)**
---
## 🔐 Variables de Entorno

La API utiliza las siguientes variables de entorno para conectarse a la base de datos:

```csharp
"HOST_VARIABLE" //IP, LOCALHOST o URL de donde se levanto la Base De datos
"DATABASE_VARIABLE" //Nombre de la base de datos que se configuro
"USERNAME_VARIABLE" //UserName de la Base De datos
"PASSWORD_VARIABLE" //Password de la Base De datos
```
---
### 🛠️ Orden de ejecución recomendado

1. **⚙️ Crear la base de datos**  
   Configura y crea una base de datos PostgreSQL con los nombres, usuarios y contraseñas que desees.

2. **⚙️ Configurar el entorno de la API**  
   Define las siguientes variables de entorno en tu sistema:

   ```txt
   HOST_VARIABLE
   DATABASE_VARIABLE
   USERNAME_VARIABLE
   PASSWORD_VARIABLE
   
3. **⚙️ Ejecutar y probar la API**  
   Levanta la API con dotnet run (o utilizando algún servidor web como IIS, Nginx, o mediante Docker) y verifica que se conecte correctamente a la base de datos.
