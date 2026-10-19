
CREATE TABLE Clients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    Surname NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Region NVARCHAR(100) NOT NULL
);

CREATE TABLE Contracts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    ServiceLevel NVARCHAR(100) NOT NULL,
    FilePath NVARCHAR(255),
    FOREIGN KEY (ClientId) REFERENCES Clients(Id)
);

CREATE TABLE ServiceRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ContractId INT NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Cost DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    FOREIGN KEY (ContractId) REFERENCES Contracts(Id)
);

-- Fictional demonstration records. Contact details are placeholders only.
INSERT INTO Clients (FirstName, Surname, Email, PhoneNumber, Region) VALUES
('Demo', 'Client One', 'client1@example.com', '0000000001', 'KwaZulu-Natal'),
('Demo', 'Client Two', 'client2@example.com', '0000000002', 'Gauteng'),
('Demo', 'Client Three', 'client3@example.com', '0000000003', 'Western Cape'),
('Demo', 'Client Four', 'client4@example.com', '0000000004', 'Eastern Cape'),
('Demo', 'Client Five', 'client5@example.com', '0000000005', 'Free State');