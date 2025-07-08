// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5229/api',  // HTTP for local development

    // Alternative shortcut endpoints
  attorneyApiUrl: 'https://localhost:5229/api/Attorney',
  caseApiUrl: 'https://localhost:5229/api/cases',
  deadlineApiUrl: 'https://localhost:5229/api/deadlines',
  analyticsApiUrl: 'https://localhost:5229/api/analytics',
  documentApiUrl: 'https://localhost:5229/api/documents',
  personApiUrl: 'https://localhost:5229/api/people'
};
