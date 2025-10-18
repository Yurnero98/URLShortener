# How the URL Shortening Algorithm Works

1. The user submits an original long URL, for example:
   `https://example.com/some/very/long/link`.

2. The algorithm generates a **SHA256 hash** of this URL.
   This produces a unique and deterministic sequence of bytes.

3. The first few characters of the hash (usually 8) are converted
   into a short **code** such as `a1b2c3d4`.

4. The short code and the original URL are stored in the database.
   If the same original URL already exists, the same short code is returned.

5. When a user visits `https://yourdomain.com/a1b2c3d4`,
   the system looks up the code in the database and performs
   an HTTP **redirect** to the original URL.

## Advantages
- Each code is **unique** due to the hashing algorithm.
- Fast lookups thanks to database indexing.
- Duplicate prevention — identical URLs return the same code.
- Stateless and scalable: no external dependencies are required.