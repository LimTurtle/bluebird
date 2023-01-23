#pragma once

#define THREAD_SIZE 10
#define ROOM_COUNT 20

struct Vector3
{
	float x;
	float y;
	float z;
};

template<typename Packet_Type, typename Header_Type>
inline Packet_Type ParsingPacket(BYTE* buffer, int32 len)
{
	Packet_Type pkt;
	pkt.ParseFromArray(buffer + sizeof(Header_Type), len);

	return pkt;
}

template<typename T, typename Header, typename S>
inline SendBufferRef _MakeSendBuffer(T& pkt, S type)
{
	const uint16 dataSize = static_cast<uint16>(pkt.ByteSizeLong());
	const uint16 packetSize = dataSize + sizeof(Header);

	SendBufferRef sendBuffer = GSendBufferManager->Open(packetSize);
	Header* header = reinterpret_cast<Header*>(sendBuffer->Buffer());
	header->size = dataSize;
	header->type = type;
	ASSERT_CRASH(pkt.SerializeToArray(&header[1], dataSize));
	sendBuffer->Close(packetSize);

	return sendBuffer;
}