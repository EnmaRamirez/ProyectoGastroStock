-- 1. Crear la base de datos
CREATE DATABASE GastroStock;
GO

USE GastroStock;
GO

-- 2. Tabla de Usuarios
CREATE TABLE Usuarios (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    usuario VARCHAR(50) NOT NULL UNIQUE,
    contrasenia VARCHAR(255) NOT NULL,
    rol VARCHAR(20) NOT NULL CHECK (rol IN ('Administrador', 'Encargado de Cocina'))
);

-- 3. Tabla de Ingredientes
CREATE TABLE Ingredientes (
    id_ingrediente INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    stock_actual DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    stock_minimo DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    fecha_vencimiento DATE NOT NULL
);

-- 4. Tabla de Movimientos de Inventario
CREATE TABLE Movimientos (
    id_movimiento INT IDENTITY(1,1) PRIMARY KEY,
    id_ingrediente INT FOREIGN KEY REFERENCES Ingredientes(id_ingrediente),
    id_usuario INT FOREIGN KEY REFERENCES Usuarios(id_usuario),
    tipo_movimiento VARCHAR(20) NOT NULL CHECK (tipo_movimiento IN ('Entrada', 'Salida')),
    cantidad DECIMAL(10,2) NOT NULL,
    fecha_registro DATETIME DEFAULT GETDATE(),
    motivo VARCHAR(255) NULL
);

USE GastroStock;
GO

-- Insertar usuario de prueba (Administrador)
INSERT INTO Usuarios (nombre, usuario, contrasenia, rol)
VALUES ('Enma Ramírez', 'enma_admin', '12345', 'Administrador');

-- Insertar ingredientes de prueba (con datos normales, bajos de stock y próximos a vencer)
INSERT INTO Ingredientes (nombre, stock_actual, stock_minimo, fecha_vencimiento)
VALUES 
('Pechuga de Pollo (Kg)', 25.00, 5.00, '2026-11-15'),
('Tomate Manzano (Kg)', 2.00, 6.00, '2026-10-25'),
('Crema Fresca (Litros)', 10.00, 2.00, '2026-10-14');

USE GastroStock;
GO

-- Registramos las cargas iniciales de tus 3 productos base vinculados a tu usuario Administrador (id_usuario = 1)
INSERT INTO Movimientos (id_ingrediente, id_usuario, tipo_movimiento, cantidad, motivo)
VALUES 
(1, 1, 'Entrada', 25.00, 'Inventario Inicial de Pechuga de Pollo'),
(2, 1, 'Entrada', 2.00, 'Inventario Inicial de Tomate Manzano'),
(3, 1, 'Entrada', 10.00, 'Inventario Inicial de Crema Fresca');

