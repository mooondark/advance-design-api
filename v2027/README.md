# Advance Design API 2027 BETA

The **Advance Design API** is a REST API that allows you to automate [Advance Design](https://www.graitec.com/advance-design/) operations programmatically — create projects, define materials, sections and elements, apply loads, run analyses and retrieve results — all without opening the Advance Design UI.

This repository provides everything you need to get started: an OpenAPI schema, documentation, ready-to-run C# samples and a Python client.

---

## Repository Structure

```
v2027/
├── docs/                   # Documentation
│   ├── HELP.md             # Full developer reference (endpoints, data models, enumerations, examples)
│   └── Summary.md          # Condensed capabilities summary
├── Samples/
│   └── Test.API.Client/    # C# .NET 8 sample console application
├── Python/
│   ├── client/
│   │   └── PythonClient.zip    # Python API client generated with Microsoft Kiota
│   └── sample/
│       └── samplePython.zip    # Ready-to-run Python sample
└── schema OpenAPI/
    └── swagger.json        # OpenAPI 3.0 specification (Swagger)
```

---

## Getting Started

### 1. Explore the Documentation

| Document | Description |
|---|---|
| [`docs/Summary.md`](docs/Summary.md) | Quick overview of all available endpoints and supported types |
| [`docs/HELP.md`](docs/HELP.md) | Comprehensive developer reference — endpoints, request/response models, enumerations, error handling and full C# usage examples |

### 2. Run the C# Samples

The [`Samples/Test.API.Client`](Samples/Test.API.Client) folder contains a **.NET 8 console application** with multiple ready-to-run scenarios:

- **Analysis results** — portal frames, slabs, imposed displacements, support reaction verification
- **Climatic load generation** — wind (EN 1991-1-4), snow (EN 1991-1-3), IBC/ASCE 7-22
- **Planar elements** — walls and slabs with supports and results
- **Error handling** — intentional material errors and custom properties on reopen

See the [Samples README](Samples/Test.API.Client/README.md) for a detailed description of each sample.

### 3. Use the Python Client

The [`Python`](Python) folder contains:

| File | Description |
|---|---|
| [`Python/client/PythonClient.zip`](Python/client/PythonClient.zip) | A Python API client generated with [Microsoft Kiota](https://learn.microsoft.com/en-us/openapi/kiota/overview) |
| [`Python/sample/samplePython.zip`](Python/sample/samplePython.zip) | A sample Python project demonstrating how to use the generated client |

Extract the archives and follow the instructions inside to get started.

### 4. Generate Your Own Client

You are free to use the OpenAPI schema located at [`schema OpenAPI/swagger.json`](schema%20OpenAPI/swagger.json) to generate a client in any language or framework of your choice. Tools such as [Microsoft Kiota](https://learn.microsoft.com/en-us/openapi/kiota/overview), [NSwag](https://github.com/RicoSuter/NSwag), [OpenAPI Generator](https://openapi-generator.tech/) and others can consume this file directly.

---

## OpenAPI Schema

The full API contract is described in the **OpenAPI 3.0** specification file:

```
schema OpenAPI/swagger.json
```

Import it into tools like **Swagger UI**, **Postman** or any OpenAPI-compatible client to browse endpoints, inspect request/response schemas and try out calls interactively.

---

## Prerequisites

- **Advance Design 2027** installed (the API server ships with the product)
- **.NET 8 SDK** (for the C# samples)
- **Python 3.10+** (for the Python client and sample)

---

## License

Please refer to the GRAITEC Advance Design license terms for usage of this API.
