#include "pch.h"
#include "MatchSession.h"
#include "MatchHandler.h"

void MatchSession::OnConnected()
{
	cout << "Connected" << endl;
}

void MatchSession::OnDisconnected()
{

}

void MatchSession::OnRecvPacket(BYTE* buffer, int32 len)
{
	PacketSessionRef match_ref = static_pointer_cast<PacketSession>(shared_from_this());
	MatchHandler::HandlerPacket(match_ref, buffer, len);
}

void MatchSession::OnSend(int32 len)
{

}