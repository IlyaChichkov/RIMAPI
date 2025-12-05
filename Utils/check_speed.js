import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<200'],
  },
};

export default function () {
  const url = 'http://localhost:8765/api/v1/colonists/detailed';
  const params = {
    timeout: '30s',
    tags: { name: 'colonists_endpoint' },
  };

  const res = http.get(url, params);
  
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
    'success is true': (r) => {
      try {
        const json = r.json();
        return json.success === true;
      } catch (e) {
        return false;
      }
    },
    'has data array': (r) => {
      try {
        const json = r.json();
        return Array.isArray(json.data) && json.data.length > 0;
      } catch (e) {
        return false;
      }
    },
    'no errors': (r) => {
      try {
        const json = r.json();
        return Array.isArray(json.errors) && json.errors.length === 0;
      } catch (e) {
        return false;
      }
    },
  });
  
  sleep(1);
}