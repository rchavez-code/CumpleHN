/* ============================================================
   CumpleHN — 01. Creación de la base de datos
   ------------------------------------------------------------
   Instancia: localhost\SQLEXPRESS
   Base:      BDCUMPLEHN

   Ejecutar una sola vez, antes que cualquier otro script.
   ============================================================ */

IF DB_ID('BDCUMPLEHN') IS NULL
BEGIN
    CREATE DATABASE BDCUMPLEHN;
END
GO

USE BDCUMPLEHN;
GO

/* Español de Honduras para las comparaciones de texto: hace que las
   búsquedas por nombre no distingan mayúsculas ni acentos. */
IF (SELECT collation_name FROM sys.databases WHERE name = 'BDCUMPLEHN')
   <> 'Modern_Spanish_CI_AI'
BEGIN
    ALTER DATABASE BDCUMPLEHN COLLATE Modern_Spanish_CI_AI;
END
GO
