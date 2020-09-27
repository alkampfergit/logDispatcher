# ingress test

To test ingress just launch ingress.py and then try to do a POST on /upload path with a content like this

```json
{
    "source" : "bla",
    "destination" : "bar",
    "logs" : [{
        "event" : "sample",
        "data" : 42
    },
    {
        "event" : "sample2",
        "data" : 1004
    }
    ]
}
```

As an example you can use curl

```bash
curl --location --request POST 'http://127.0.0.1:5000/upload' \
--header 'Content-Type: application/json' \
--data-raw '{
    "source" : "bla",
    "destination" : "bar",
    "logs" : [{
        "event" : "sample",
        "data" : 42
    },
    {
        "event" : "sample2",
        "data" : 1004
    }
    ]
}'
```