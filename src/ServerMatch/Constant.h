#pragma once

#define THREAD_SIZE 10
#define PLATER_COUNT 5
#define MAX_PLAYER_COUNT 10

#define TestLevel 5
#define TestRoom 10

struct MatchHeader
{
	google::protobuf::uint32 size;
	Match::STATE state;
};

template<typename Packet_Type, typename Header_Type>
Packet_Type _Parsing(BYTE* buffer, int32 len)
{
	Packet_Type pkt;
	pkt.ParseFromArray(buffer + sizeof(Header_Type), len);

	return pkt;
}

template<typename T, typename Header, typename S>
SendBufferRef _Making(T& pkt, S type)
{
	const uint16 dataSize = static_cast<uint16>(pkt.ByteSizeLong());
	const uint16 packetSize = dataSize + sizeof(Header);

	SendBufferRef sendBuffer = GSendBufferManager->Open(packetSize);
	Header* header = reinterpret_cast<Header*>(sendBuffer->Buffer());
	header->size = dataSize;
	header->state = type;
	ASSERT_CRASH(pkt.SerializeToArray(&header[1], dataSize));
	sendBuffer->Close(packetSize);

	return sendBuffer;
}