export type Packet = {
    opCode: number
    name: string
    direction: string
    length: number
    connectionIndex: number
    endpointAddress: string
    endpointPort: number
    number: number
    timestamp: number
    message: string
}