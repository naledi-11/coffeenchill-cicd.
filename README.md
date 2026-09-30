# CoffeeNChill — Azure Functions API

## Project status

Member 1 (Menu API). - 5 endpoints utilizing Azure Table Storage.
Member 2 (Staff Documents). - 3 endpoints across Azure Blob Storage (refer to "Azure File Share vs Blob Storage" below for reasons why Blob Storage was utilized in place of a File Share.
member 3 (Docker/Containerization). -Dockerized the Functions app with Azurite running as a separete containder; image built and pushed to Docker Hub as coffeenchillmember3/coffeenchill-functions:v1.0.

Postman
1. CoffeeNChill.postman_collection.json = just requests, no tests. 
2.CoffeeNChill - Part 1.postman_collection_tests.json.= this is the one with test.

## Running the project
1. Start Azurite (see above) and leave it running.
2. Open the project in Visual Studio and run it (or `func start` /
   `dotnet run` from the `CoffeeNChill` folder).
3. The API listens on `http://localhost:7071/api/...` by default (adjust
   if your environment uses a different host/port).



## Endpoints

### Menu (Member 1 — Azure Table Storage, table `MenuItems`)

| Method | Route                              |
|--------|-------------------------------------|
| POST   | `/api/menu`                         |
| GET    | `/api/menu`                         |
| GET    | `/api/menu/category/{category}`     |
| PUT    | `/api/menu/{category}/{id}`         |
| DELETE | `/api/menu/{category}/{id}`         |

`category` = PartitionKey, `id` = RowKey.

### Documents (Member 2 — Azure Blob Storage, container `staff-docs`)

| Method | Route                                   |
|--------|-------------------------------------------|
| POST   | `/api/documents/upload`                   |
| GET    | `/api/documents`                          |
| GET    | `/api/documents/download/{fileName}`      |

 Upload — send `multipart/form-data` with a form field named `file`.
 Returns `{ fileName, sizeBytes, uploadedUtc }`.
 List — returns an array of `{ fileName, sizeInBytes, lastModified }`
 for every document currently stored.
 Download — streams the file back with the correct content type and
 `Content-Disposition: attachment; filename=...`. Returns 404 if the
 file doesn't exist.

## Azure File Share vs Blob Storage — why the substitution had to be made


The assignment brief asks for staff documents to be stored in an Azure
File Share. This project, on the other hand, saves them in an Azure Blob.
container called `staff-docs`. This is a calculated, recorded choice.
not a mistake:
Azurite does not replicate the Azure Files service. Azurite solely
simulates Blob, Queue, and Table storage. Indicating a `ShareServiceClient`
at `UseDevelopmentStorage=true` does not connect, and there is no
setup that resolves this — the service is just not implemented
locally (monitored upstream since 2018: https://github.com/Azure/Azurite/issues/113).
As this project is created and evaluated solely with Azurite (no
group member possesses an Azure subscription for this course), an actual File
Sharing was not a choice for local development.
A Blob container provides a similar experience for this functionality —
uploading stream, listing by name/size/date-modified, and streaming
download — and the identical code operates unchanged with Azurite
locally and an actual Azure Storage account in production. If your pen
specifically needs an actual File Share; the other option is to direct
`AzureWebJobsStorage` in an actual Azure Storage account (bypassing Azurite)
for this specific feature only); the code would require a `ShareServiceClient`
instead of `BlobServiceClient` in `BlobStorageService`, yet the function
signatures and paths remain unchanged.

## Testing (Member 2 handover)
Prior to beginning Document work, the Menu endpoints for Member 1 were executed again.
against a new Azurite instance and verified functioning (all 5 endpoints).

To test the Document endpoints in Postman:

1. Upload — `POST /api/documents/upload`, Body → form-data → key
   `file`, type `File`, pick any document from your machine.
2. List — `GET /api/documents` — confirm the uploaded file appears
   with the correct size and a recent `lastModified`.
3. Download — `GET /api/documents/download/{fileName}` — confirm the
   file downloads and opens correctly.
4. Error cases — try uploading with no file (expect 400), and
   downloading a file name that doesn't exist (expect 404).
5. Regression check — re-run all 5 Menu endpoints again to confirm
   they still work alongside the new Document endpoints.


## Known issues / notes for Member 3

`TargetFramework` is `net10.0`. Double-check the Azure Functions Docker
  base images referenced in the Dockerfile
  (`mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0`)
  are available/stable before building — if not, the whole solution may
  need to be retargeted to `net8.0` (the LTS version Azure Functions
  officially supports at time of writing), which would also mean
  changing the base images in the Dockerfile.
Both `MenuItem` and Table/Blob services read the connection string from
  the single `AzureWebJobsStorage` setting, so no extra configuration is
  needed to make Documents work alongside Menu — same storage account,
  different services within it.
The `staff-docs` blob container is created automatically on first
  request if it doesn't already exist (`CreateIfNotExists()`), same
  pattern as the `MenuItems` table.

  ## Member 3 – Menu API Verification

The five Menu API endpoints were tested successfully using Postman. Each endpoint was verified to ensure that it returned the expected response and performed its intended operation.

The following endpoints were tested:

1. **POST** `/api/menu` – Creates a new menu item.
2. **GET** `/api/menu` – Retrieves all menu items.
3. **GET** `/api/menu/category/{category}` – Retrieves menu items filtered by category.
4. **PUT** `/api/menu/{category}/{id}` – Updates an existing menu item.
5. **DELETE** `/api/menu/{category}/{id}` – Deletes a menu item.

All five endpoints returned the expected responses during testing.

## Member 3 – Document API Verification

The Document API endpoints were tested successfully using Postman. The tests verified file upload, file listing, file download, and error handling functionality.

### Document API Tests

1. **POST** `/api/documents/upload` – Upload a document  
   - A document was uploaded successfully using `multipart/form-data`.
   - The uploaded file was `stuff-manual.txt`.
   - The API returned **200 OK**.
   - The file was stored in the Azurite `staff-docs` container.

2. **GET** `/api/documents` – List uploaded documents  
   - The API returned **200 OK**.
   - The uploaded file appeared in the response with the correct file name and size.
   - The response also included a recent `lastModified` timestamp.

3. **GET** `/api/documents/download/{fileName}` – Download a document  
   - The uploaded `stuff-manual.txt` file was downloaded successfully.
   - The API returned **200 OK** and the correct file contents.

4. **Error handling**  
   - Uploading without providing a file was tested and expected to return **400 Bad Request**.
   - Attempting to download a file that does not exist was tested and expected to return **404 Not Found**.

5. **Regression testing**  
   - The existing Menu API endpoints were re-tested to confirm that they continued to function correctly alongside the new Document API endpoints.

All Document API tests returned the expected results during Postman verification.

## Known Issues / Notes

- The project currently targets **.NET 10.0**.
- The Azure Functions Docker base images referenced in the `Dockerfile` should be verified to ensure that they are compatible with the project's target framework.

## Docker Setup

Build the funtion image:
docker build -t coffeenchill-functions:v1.0 -f Dockerfile .

Create a shared network:
docker network create coffeenchill-net

Run Azurite:
docker run -d --name azurite --network coffeenchill-net -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

Run the Function container (connected to Azurite)
docker run -d --name coffeenchill-functions --network coffeenchill-net -p 7071:80 -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;QueueEndpoint=http://azurite:10001/devstoreaccount1;TableEndpoint=http://azurite:10002/devstoreaccount1;" -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" coffeenchill-functions:v1.0

Verified: GET http://localhost:7071/api/menu return 200 OK.

Log in to Docker Hub:
docker login

Tag the image for Docker Hub:
docker tag coffeenchill-functions:v1.0 coffeenchillmember3/coffeenchill-functions:v1.0

Push the image:
docker push coffeenchillmember3/coffeenchill-functions:v1.0

Images pushed to Docker Hub:
- Functions: https://hub.docker.com/r/coffeenchillmember3/coffeenchill-functions
- Azurite: https://hub.docker.com/r/coffeenchillmember3/coffeenchill-azurite

  //referenced README.md Template
 