$baseUrl = "http://localhost:5000"

# 1. Helper function for Login
function Login($email, $password) {
    $body = @{
        email = $email
        password = $password
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
        return $response.data.token
    } catch {
        Write-Error ("Login failed for " + $email + ": " + $_)
        return $null
    }
}

# 2. Login SuperAdmin
Write-Host "`n=========================================="
Write-Host "TEST 1: Authentication & Roles Verification"
Write-Host "=========================================="
$adminToken = Login "admin@smartattendance.local" "Admin@123456"
Write-Host "SuperAdmin Token retrieved successfully: $(!([string]::IsNullOrEmpty($adminToken)))"

# 3. Create a test employee user (if not existing) or use existing user
$connStr = "Server=.\SQLEXPRESS;Database=SmartAttendanceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$conn = New-Object Microsoft.Data.SqlClient.SqlConnection($connStr)
if (-not (Get-Module -ListAvailable -Name Microsoft.Data.SqlClient)) {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
}
$conn.Open()

# Get CompanyId
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Id FROM Companies"
$companyId = $cmd.ExecuteScalar().ToString()

# Register a test Employee and test User B
$empEmail = "testemployee@smartattendance.local"
$userBEmail = "testuserb@smartattendance.local"

$regBodyA = @{
    email = $empEmail
    password = "User@123456"
    firstName = "Test"
    lastName = "Employee"
    companyId = $companyId
} | ConvertTo-Json

$regBodyB = @{
    email = $userBEmail
    password = "User@123456"
    firstName = "Test"
    lastName = "UserB"
    companyId = $companyId
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/register" -Method Post -Body $regBodyA -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
} catch {}
try {
    Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/register" -Method Post -Body $regBodyB -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
} catch {}

$employeeToken = Login $empEmail "User@123456"
$userBToken = Login $userBEmail "User@123456"
Write-Host "Employee Token retrieved: $(!([string]::IsNullOrEmpty($employeeToken)))"
Write-Host "User B Token retrieved: $(!([string]::IsNullOrEmpty($userBToken)))"

# 4. Test AttendanceDevices Permissions
Write-Host "`n=========================================="
Write-Host "TEST 2: AttendanceDevices Authorization (RBAC)"
Write-Host "=========================================="

# Test with Employee (should get 403 Forbidden)
try {
    $empRes = Invoke-WebRequest -Uri "$baseUrl/api/v1/attendancedevices" -Method Get -Headers @{ Authorization = "Bearer $employeeToken" } -SkipHttpErrorCheck
    Write-Host "Employee accessing AttendanceDevices -> HTTP Status: $($empRes.StatusCode) (Expected: 403)"
} catch {
    Write-Host "Employee accessing AttendanceDevices -> Error: $($_.Exception.Message)"
}

# Test with SuperAdmin (should get 200 OK)
try {
    $adminRes = Invoke-WebRequest -Uri "$baseUrl/api/v1/attendancedevices" -Method Get -Headers @{ Authorization = "Bearer $adminToken" } -SkipHttpErrorCheck
    Write-Host "SuperAdmin accessing AttendanceDevices -> HTTP Status: $($adminRes.StatusCode) (Expected: 200)"
} catch {
    Write-Host "SuperAdmin accessing AttendanceDevices -> Error: $($_.Exception.Message)"
}

# 5. Test Notifications IDOR and Ownership Scoping
Write-Host "`n=========================================="
Write-Host "TEST 3: Notifications IDOR & Scoping Tests"
Write-Host "=========================================="

# Get User IDs from DB
$cmd.CommandText = "SELECT Id FROM AspNetUsers WHERE Email = '$empEmail'"
$userAId = $cmd.ExecuteScalar().ToString()

$cmd.CommandText = "SELECT Id FROM AspNetUsers WHERE Email = '$userBEmail'"
$userBId = $cmd.ExecuteScalar().ToString()

# Insert notification for User A and notification for User B directly in DB
$notifAId = [Guid]::NewGuid().ToString()
$notifBId = [Guid]::NewGuid().ToString()

$cmd.CommandText = @"
INSERT INTO Notifications (Id, CompanyId, RecipientUserId, Title, Message, NotificationType, Status, Priority, CreatedAt, IsDeleted)
VALUES 
('$notifAId', '$companyId', '$userAId', 'Notif for User A', 'Secret content for A', 0, 0, 1, GETUTCDATE(), 0),
('$notifBId', '$companyId', '$userBId', 'Notif for User B', 'Secret content for B', 0, 0, 1, GETUTCDATE(), 0)
"@
$cmd.ExecuteNonQuery() | Out-Null
Write-Host "Created test notifications: Notif A ($notifAId) for User A, Notif B ($notifBId) for User B"

# Test 3.1: User A requests their own notification (GET /notifications/{notifAId}) -> Expected: 200 OK
$resOwn = Invoke-WebRequest -Uri "$baseUrl/api/v1/notifications/$notifAId" -Method Get -Headers @{ Authorization = "Bearer $employeeToken" } -SkipHttpErrorCheck
Write-Host "User A reading own Notification A -> HTTP Status: $($resOwn.StatusCode), Content: $($resOwn.Content)"

# Test 3.2: User A attempts to read User B's notification (GET /notifications/{notifBId}) -> Expected: 400 Failure (Notification not found)
$resIDOR = Invoke-WebRequest -Uri "$baseUrl/api/v1/notifications/$notifBId" -Method Get -Headers @{ Authorization = "Bearer $employeeToken" } -SkipHttpErrorCheck
Write-Host "User A attempting IDOR on Notification B -> HTTP Status: $($resIDOR.StatusCode), Content: $($resIDOR.Content)"

# Test 3.3: User A attempts to mark User B's notification as read (POST /notifications/{notifBId}/read) -> Expected: 400 Failure
$resReadIDOR = Invoke-WebRequest -Uri "$baseUrl/api/v1/notifications/$notifBId/read" -Method Post -Headers @{ Authorization = "Bearer $employeeToken" } -SkipHttpErrorCheck
Write-Host "User A attempting IDOR MarkAsRead on Notification B -> HTTP Status: $($resReadIDOR.StatusCode), Content: $($resReadIDOR.Content)"

# Test 3.4: User A attempts to DELETE User B's notification (DELETE /notifications/{notifBId}) -> Expected: 400 Failure
$resDelIDOR = Invoke-WebRequest -Uri "$baseUrl/api/v1/notifications/$notifBId" -Method Delete -Headers @{ Authorization = "Bearer $employeeToken" } -SkipHttpErrorCheck
Write-Host "User A attempting IDOR DELETE on Notification B -> HTTP Status: $($resDelIDOR.StatusCode), Content: $($resDelIDOR.Content)"

# Test 3.5: User A deletes their own notification (DELETE /notifications/{notifAId}) -> Expected: 200 OK
$resDelOwn = Invoke-WebRequest -Uri "$baseUrl/api/v1/notifications/$notifAId" -Method Delete -Headers @{ Authorization = "Bearer $employeeToken" } -SkipHttpErrorCheck
Write-Host "User A deleting own Notification A -> HTTP Status: $($resDelOwn.StatusCode), Content: $($resDelOwn.Content)"

# Cleanup test notifications
$cmd.CommandText = "DELETE FROM Notifications WHERE Id IN ('$notifAId', '$notifBId')"
$cmd.ExecuteNonQuery() | Out-Null

$conn.Close()
Write-Host "`nAll Verification Tests Completed."
