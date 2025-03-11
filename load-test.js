import { randomIntBetween, randomString } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';;

import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = 'https://localhost:7161'; // Replace with your API endpoint
const RECEIVE_BID_ENDPOINT = `${BASE_URL}/api/bids/receive`;
const RPS = 10;
const DURATION = '5s'; 
const payload = JSON.stringify({
    Amount: randomIntBetween(1000, 1500),
    CarId: 1,  
    ClientEmail: `${randomString(10)}@example.com`,
    BidTime: new Date().toISOString(),
});


// Options
export const options = {
    stages: [
        { duration: DURATION, target: RPS }, 
    ],
};

// Main test function
export default function () {
    const headers = { 'Content-Type': 'application/json' };
    const response = http.post('https://localhost:7161/BidFlow', payload, { headers });
    console.log('Response Status:', response.status);
    console.log('Response Body:', response.body);
    check(response, {
       'Status is 200 OK': (r) => r.status === 200,
        'Success message in body': (r) => r.body && r.body.includes('Success reception of bid'),

        // Failure case for 400 BadRequest
        'Status is 400 BadRequest': (r) => r.status === 400,
        'BadRequest message includes validation or error': (r) => r.body && r.body.length > 0,

        // Internal server errors
        'Status is 500 InternalServerError': (r) => r.status === 500,
        'Error message in body': (r) => r.body && r.body.length > 0,
    });

    sleep(1);
}