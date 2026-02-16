# TiendaPromElec API - Reto Técnico .NET 8

Este proyecto consiste en la refactorización, aseguramiento y despliegue de una API REST para comercio electrónico desarrollada en **.NET 8 LTS**.

El objetivo principal fue transformar una aplicación base en una solución robusta, implementando **Arquitectura Limpia**, **Seguridad (OWASP)**, **Pruebas Automatizadas** y **Contenerización con Docker**.

---

## Características Implementadas

* **Arquitectura:** Separación de responsabilidades mediante el **Patrón Repositorio**.
* **Seguridad (OWASP):**
    * Autenticación y Autorización vía **JWT (Bearer Token)**.
    * Hashing de contraseñas seguro con **BCrypt**.
    * Políticas de **CORS** restrictivas.
    * Protección contra inyección (Validación de modelos).
* **Calidad:** Suite de pruebas completa (**Unitarias** con Moq y **de Integración** en Memoria).
* **Despliegue:** Configuración lista para **Docker** y orquestación con **Nginx** como Proxy Inverso.

---

## Prerrequisitos

Para ejecutar este proyecto necesitas tener instalado:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)
* SQL Server o Postgres(Local, o en la nube o en Docker)

---

## Configuración Inicial

Antes de ejecutar la API, asegúrate de configurar la cadena de conexión a tu base de datos en el archivo `appsettings.json` (dentro de la carpeta `TiendaPromElec`).

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=TiendaDB;User Id=sa;Password=TuPasswordFuerte;TrustServerCertificate=True;"
},
"JWT_SECRET": "TuSuperSecretoParaDesarrollo_1234567890"
```
---

## Instrucciones de Ejecución
1. Restaurar Dependencias
Abre una terminal en la carpeta raíz de la solución y ejecuta:

```bash
dotnet restore
```

2. Ejecutar la API (Localmente)
Para iniciar el servidor de desarrollo:

```bash
cd TiendaPromElec
dotnet run
```

La API estará disponible en:

Swagger UI: http://localhost:7131/swagger (o el puerto que indique tu consola).

3. Ejecutar las Pruebas (Testing) 🧪
Para ejecutar la suite completa de pruebas (Unitarias e Integración) y validar la cobertura en la carpeta raiz ejecutar los siguientes comandos en la terminal:

```bash
cd TiendaPromElec.Test
dotnet test
```

---

## Construcción y Ejecución en Docker
Este proyecto está configurado para funcionar levantando la API y un servidor Nginx como Proxy Inverso.

Opción A: Accede a la aplicación a través de Nginx:

URL: http://localhost/swagger (Puerto 80)

(Si configuraste SSL en nginx.conf): https://localhost/swagger

Opción B: Ejecutar solo el contenedor de la API
Si prefieres ejecutar solo la API sin Nginx:

1- Construir la imagen:

```bash
docker build -t tiendapromelec-image .
```

2- Ejecutar el contenedor (Mapeando el puerto 8080 del contenedor al 8080 de tu PC):

```bash
docker run -d --name TiendaPromElec tiendapromelec-image -p 8080:8080
```

Acceder: http://localhost:8080/swagger

---

## Repositorio
El código fuente completo y el historial de cambios se encuentra disponible en GitHub:
https://github.com/cseduardo/TiendaPromElec

Desarrollado por: Jose Eduardo Campos Sanchez