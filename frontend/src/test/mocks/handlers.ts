import { http, HttpResponse } from 'msw';

// Replace with your actual backend URL or use environment variables
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

export const handlers = [
  // Fallback handler for unhandled requests if needed
  http.get(`${BASE_URL}/*`, () => {
    return HttpResponse.json({ message: 'Not Found' }, { status: 404 });
  }),
];
