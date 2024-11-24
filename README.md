# File Management API

## About the Project
The **File Management API** is a .NET Core-based backend application designed for efficient document management. It leverages the **Google Drive API** for storage and retrieval of files, allowing users to upload, download, share, and view document metadata. The application implements **memory caching** for lightweight download tracking.

---

## Features
- **Upload and Store**: Upload documents to Google Drive and track their metadata.
- **List and Preview**: List uploaded documents and generate preview links for the first page of documents.
- **Download and Share**: Download documents or create shareable public links with expiration periods.
- **Memory Caching**: Tracks download counts without requiring persistent storage.

---

## Technologies Used
- **.NET Core 8.0**: Framework for building scalable and high-performance APIs.
- **Google Drive API**: For document storage, retrieval, and metadata management.
- **Memory Cache**: Lightweight solution for tracking download counts during application runtime.

---

## API Endpoints

1. **Upload Documents (`POST /upload`)**  
   Upload one or more documents to the API, which stores them on Google Drive.

2. **List Documents (`GET /list`)**  
   Retrieve a list of all uploaded documents along with their metadata.

3. **Download Document (`GET /download/{fileId}`)**  
   Fetch and download a specific document by its unique identifier.

4. **Share Document (`POST /share/{fileId}`)**  
   Generate a public shareable link with an expiration period. Default expiration is set to 1 hour.

5. **Get Document Preview Link (`GET /preview-link/{fileId}`)**  
   Retrieve a link to preview the first page of the document.

6. **Delete All Documents (`DELETE /delete-all`)**  
   Remove all documents from Google Drive via a bulk delete operation.(This endpoint is intended for manual operations and should be used with caution to avoid unintended data loss.)

---

## Instructions on How to Run & Test the Application

### Prerequisites
- **.NET Core SDK**: Ensure .NET Core 8.0 or higher is installed on your system.
- **Google Drive API Setup**:
  - Enable the Google Drive API in your Google Cloud Console.
  - Download the `credentials.json` file and place it in the project’s `wwwroot` directory.

### Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/mhmtcrkglu/file-management-api.git
   cd file-management-api
   ```

2. Restore dependencies and run the project:
   ```bash
   dotnet restore
   dotnet run
   ```

3. Access the API endpoints at `http://localhost:5000`.

### Testing the Application
1. **Uploading Files**: Use a REST client (e.g., Postman) to test file uploads via the `/upload` endpoint.
2. **Fetching Document List**: Retrieve and verify metadata of uploaded files with `/list`.
3. **Downloading Documents**: Ensure files download correctly via the `/download/{fileId}` endpoint.
4. **Sharing Documents**: Test shareable link generation with `/share/{fileId}` and verify expiration behavior.
5. **Preview Links**: Confirm that `/preview-link/{fileId}` returns valid links for document previews.

---

## Architecture and Design Decisions

1. **Google Drive Integration**
   - Chosen for scalability and security, Google Drive serves as the document storage backend, ensuring reliable storage and retrieval.

2. **Memory Caching**
   - Used for lightweight and efficient download tracking without additional database overhead. The trade-off is that download counts reset upon application restart.

3. **RESTful API Design**
   - Endpoints follow REST principles for simplicity and clarity, supporting CRUD-like operations for document management.

4. **Modular Structure**
   - The project is structured to separate concerns, with individual controllers handling uploads, downloads, sharing, and previews.

5. **Expirable Sharing**
   - Shareable links are designed to expire automatically after the specified duration to enhance security and prevent overexposure of files.

---

## Ideas and Proposals for Improvement

### From a User Perspective
1. **Granular Permissions**:
   - Enable users to set permissions for shared links (e.g., view-only, edit, or download).

2. **Enhanced Metadata**:
   - Add tags, descriptions, or categories to files for easier organization.

3. **Notifications**:
   - Notify users about link expiration or access activities for shared files.

4. **Pagination for Document List**:
   - Implement pagination to handle large datasets more effectively.

---

### From a Technical Perspective
1. **Persistent Storage for Download Tracking**:
   - Replace memory caching with a database (e.g., PostgreSQL, MongoDB) for persistent download count tracking.

2. **Authentication and Authorization**:
   - Integrate OAuth2 for user authentication and role-based access control to enhance security.

3. **Logging and Monitoring**:
   - Use tools like Serilog or Elastic Stack to monitor API usage and debug issues.

4. **Cloud-Native Enhancements**:
   - Deploy the API to a containerized environment (e.g., Docker) with auto-scaling on platforms like Kubernetes or Azure App Service.

5. **Improved Test Coverage**:
   - Add unit and integration tests to ensure robust validation of API functionality and edge cases.

6. **CI/CD Pipeline Implementation**:
   - Set up a CI/CD pipeline using tools like GitHub Actions, GitLab CI, or Jenkins to automate code linting, testing, and deployment to staging/production environments. This ensures seamless updates, faster feature releases, rollback strategies, and early vulnerability detection through security scanning (e.g., Snyk, Dependabot).

---

## Notes
- **Temporary Data**: Download counts are stored in memory and reset upon application restart. Persistent storage is recommended for production.
- **Link Expiration**: Shareable links expire as per the duration specified during their creation (default: 1 hour).
- **Preview Limitations**: Document previews are limited to the first page and depend on Google Drive’s preview capabilities.

---