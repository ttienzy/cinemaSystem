-- Cinema System Database Initialization Script
-- This script creates databases if they don't exist

USE master;
GO

-- Create Booking Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Booking')
BEGIN
    CREATE DATABASE Booking;
    PRINT 'Booking database created successfully.';
END
ELSE
BEGIN
    PRINT 'Booking database already exists.';
END
GO

-- Create Identity Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Identity')
BEGIN
    CREATE DATABASE Identity;
    PRINT 'Identity database created successfully.';
END
ELSE
BEGIN
    PRINT 'Identity database already exists.';
END
GO

-- Set database options for better performance
ALTER DATABASE Booking SET RECOVERY SIMPLE;
ALTER DATABASE Identity SET RECOVERY SIMPLE;
GO

PRINT 'Database initialization completed.';
GO
