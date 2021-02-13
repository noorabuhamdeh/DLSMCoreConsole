export interface ComPort {
    id: number;
    meterId: number;
    portName: string;
    baudRate: number;
    dataBits: number;
    parity: number;
    stopBits: number;
  }
  export interface ComPort {
    id: number;
    name: string;
    objectsXMLDocument: string;
    authenticationType: number;
    password: string;
    physicalServer: number;
    logicalServer: number;
    clientAddress: number;
    manufactureName: string;
    useLogicalNameReferencing: boolean;
    interfaceType: number;
    lastForcedReadTime: string;
  }
  export interface ComPort {
    id: number;
    meterId: number;
    obiS_Code: string;
    mappedToAddress: number;
    dataType: string;
  }
