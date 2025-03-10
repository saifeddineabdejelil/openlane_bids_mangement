mport http from 'k6/http';
import { check, sleep } from 'k6';
import { randomInt, randomString, randomFloat } from 'k6';

export default function () {
   
    const bid = {
        Id: randomInt(1, 1000),
        Amount: randomFloat(10, 1000).toFixed(2),
        CarId: randomInt(1, 100),  
        ClientEmail: `${randomString(10)}@example.com`,
        BidTime: new Date().toISOString(),
    };

    const url = 'http://localhost:5000/api/bids';

    const res = http.post(url, JSON.stringify(bid), {
        headers: { 'Content-Type': 'application/json' },
    });

    check(res, {
        'status est 200': (r) => r.status === 200,
    });

    sleep(1);
}