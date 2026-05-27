import time
import json
import base64
import urllib.request
import urllib.error

PUBSUB_HOST = "pubsub:8681"
PROJECT_ID = "local-project"
SUB_NAME = f"projects/{PROJECT_ID}/subscriptions/gcs-router-sub"
TOPIC_NAME = f"projects/{PROJECT_ID}/topics/gcs-object-events"

TOPIC_MAP = {
    "uploads": "file-uploaded-topic",
    "converted": "file-converted-topic",
    "thumbnails": "thumbnail-created-topic",
    "analysis-results": "analysis-completed-topic"
}

def post(url, data):
    req = urllib.request.Request(
        url,
        data=json.dumps(data).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="POST"
    )
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode("utf-8"))

def put(url, data):
    req = urllib.request.Request(
        url,
        data=json.dumps(data).encode("utf-8"),
        headers={"Content-Type": "application/json"},
        method="PUT"
    )
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode("utf-8"))

def setup_subscription():
    url = f"http://{PUBSUB_HOST}/v1/{SUB_NAME}"
    data = {"topic": TOPIC_NAME}
    print(f"Creating subscription {SUB_NAME} on topic {TOPIC_NAME}...")
    while True:
        try:
            put(url, data)
            print("Subscription created successfully.")
            break
        except Exception as e:
            print(f"Waiting for Pub/Sub to be ready: {e}")
            time.sleep(2)

def main():
    setup_subscription()
    pull_url = f"http://{PUBSUB_HOST}/v1/{SUB_NAME}:pull"
    ack_url = f"http://{PUBSUB_HOST}/v1/{SUB_NAME}:acknowledge"
    
    while True:
        try:
            resp = post(pull_url, {"maxMessages": 10, "returnImmediately": False})
            received = resp.get("receivedMessages", [])
            if not received:
                time.sleep(0.5)
                continue
            
            ack_ids = []
            for item in received:
                msg = item.get("message", {})
                data_b64 = msg.get("data", "")
                ack_id = item.get("ackId")
                
                try:
                    payload = json.loads(base64.b64decode(data_b64).decode("utf-8"))
                    bucket = payload.get("bucket", "")
                    target = TOPIC_MAP.get(bucket)
                    if target:
                        pub_url = f"http://{PUBSUB_HOST}/v1/projects/{PROJECT_ID}/topics/{target}:publish"
                        post(pub_url, {"messages": [msg]})
                        print(f"Routed event for bucket '{bucket}' to topic '{target}'")
                except Exception as ex:
                    print(f"Error processing message: {ex}")
                
                if ack_id:
                    ack_ids.append(ack_id)
            
            if ack_ids:
                post(ack_url, {"ackIds": ack_ids})
                
        except urllib.error.URLError as e:
            print(f"Connection error: {e}")
            time.sleep(2)
        except Exception as e:
            print(f"Error in main loop: {e}")
            time.sleep(1)

if __name__ == "__main__":
    main()
