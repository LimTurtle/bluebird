#include "pch.h"
#include "GameSession.h"

void GameSession::OnConnected()
{
	cout << "Connected" << endl;
}

void GameSession::OnDisconnected()
{

}

void GameSession::OnRecvPacket(BYTE* buffer, int32 len)
{
	PacketSessionRef match_ref = static_pointer_cast<PacketSession>(shared_from_this());
	PacketHandler::HandlerPacket(match_ref, buffer, len);
}

void GameSession::OnSend(int32 len)
{

}
