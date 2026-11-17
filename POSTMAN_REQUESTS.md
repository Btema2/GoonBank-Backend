# GoonBank Postman Testing Guide

This guide contains requests ready for manual copying or one-click import into Postman.

---

## 1. Quick Import into Postman (Recommended)

A ready-to-use collection has been generated at:
- [GoonBank.postman_collection.json](file:///home/dominik/RiderProjects/jewBank/GoonBank.postman_collection.json)

### How to Import:
1. Open Postman.
2. Click **Import** (top left).
3. Drag & drop or select `GoonBank.postman_collection.json`.
4. You will get a complete folder structure with built-in scripts that **automatically store tokens, account IDs, and transaction IDs** into collection variables so you do not need to copy-paste them manually between requests.

---

## 2. Example Requests (Manual Copy & Paste)

If you prefer pasting requests manually or using cURL/Raw JSON:

### Step 1: Register Account
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/Account`
- **Headers:** `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "firstName": "Jan",
  "lastName": "Kowalski",
  "email": "jan.kowalski@example.com",
  "passwordHash": "Secret123!",
  "dateOfBirth": "1992-05-15",
  "currency": "PLN",
  "accountType": 0
}
```

---

### Step 2: Login (Get JWT Token)
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/auth/login`
- **Headers:** `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "email": "jan.kowalski@example.com",
  "password": "Secret123!"
}
```
> **Response Note:** Copy the `"token"` and `"accountId"` from the response for the following requests.

---

### Step 3: Get Current User Profile
- **Method:** `GET`
- **URL:** `http://localhost:5001/api/auth/me`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`

---

### Step 4: Create a Cash Deposit
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/Transaction`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`
  - `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "sourceAccountId": "<YOUR_ACCOUNT_ID>",
  "amount": 1000.00,
  "transactionType": 2,
  "description": "Cash ATM deposit"
}
```
> **Response Note:** Copy the `"transactionId"` from the response.

---

### Step 5: Execute the Deposit
- **Method:** `PUT`
- **URL:** `http://localhost:5001/api/Transaction/<TRANSACTION_ID>/execute`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`

---

### Step 6: Check Account Balance & Details
- **Method:** `GET`
- **URL:** `http://localhost:5001/api/Account/<YOUR_ACCOUNT_ID>`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`

---

### Step 7: Register BLIH Profile
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/BlihProfile`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`
  - `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "accountId": "<YOUR_ACCOUNT_ID>",
  "phoneHash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
}
```

---

### Step 8: Generate BLIH Code
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/Blih/code`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`
  - `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "accountId": "<YOUR_ACCOUNT_ID>"
}
```
> **Response Note:** Returns a 6-digit code valid for 2 minutes (e.g., `123456`).

---

### Step 9: Merchant Initiates BLIH Payment
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/Blih/payment/initiate`
- **Headers:** `Content-Type: application/json`
- **Body (raw JSON):**
```json
{
  "blihCode": "<6_DIGIT_BLIH_CODE>",
  "merchantId": "STORE_ZABKA_01",
  "amount": 45.50
}
```
> **Response Note:** Copy `"paymentId"` from the response.

---

### Step 10: Customer Confirms BLIH Payment
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/Blih/payment/<PAYMENT_ID>/confirm`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`

---

## 4. Testing & Cleanup (Development Mode)

These endpoints allow you to clean up or completely reset your database from Postman without restarting or manually deleting files.

### 4.1 Reset Database (Wipe All Records)
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/testing/reset`
- **Headers:** None required (`[AllowAnonymous]`)
- **Body:** None
- **Result:** Wipes all Accounts, Transactions, BLIH codes/payments, and Audit logs so you can start completely clean.

### 4.2 Delete Single Account
- **Method:** `DELETE`
- **URL:** `http://localhost:5001/api/testing/accounts/<ACCOUNT_ID>`
- **Headers:** None required
- **Body:** None
- **Result:** Removes that specific account and all of its associated transactions and BLIH profiles.

---

## 5. Logout & Audit Logs

### 5.1 Logout
- **Method:** `POST`
- **URL:** `http://localhost:5001/api/auth/logout`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`
- **Result:** Records a `UserLogout` session audit log in the database and returns `200 OK`.

### 5.2 View My Audit Logs
- **Method:** `GET`
- **URL:** `http://localhost:5001/api/audit?page=1&pageSize=50`
- **Headers:**
  - `Authorization: Bearer <YOUR_TOKEN>`
- **Result:** Returns all audit logs associated with your account (logins, logouts, transactions, status updates).

### 5.3 View All Audit Logs (Development Only)
- **Method:** `GET`
- **URL:** `http://localhost:5001/api/audit/all?page=1&pageSize=50`
- **Headers:** None (Anonymous in dev mode)
- **Result:** Returns all system audit logs across all users and actions.
