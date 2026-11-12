import sqlite3

conn = sqlite3.connect('smart_attendance.db')
c = conn.cursor()

# List all tables
c.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = c.fetchall()
print("=== Tables ===")
for t in tables:
    print(t[0])

print()

# Check AspNetUsers
try:
    c.execute("SELECT Email, UserName, IsActive, NormalizedEmail FROM AspNetUsers LIMIT 10")
    rows = c.fetchall()
    print("=== AspNetUsers ===")
    for r in rows:
        print(f"  Email={r[0]}, UserName={r[1]}, IsActive={r[2]}, NormalizedEmail={r[3]}")
except Exception as e:
    print(f"AspNetUsers error: {e}")

conn.close()
