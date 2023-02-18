#include "pch.h"
#include "GameSession.h"
#include "NpcSession.h"
#include "MatchSession.h"
#include <ThreadManager.h>

enum
{
	WORKER_TICK = 64
};

template<typename T>
void DoWorkerJob(T& service)
{
	while (true)
	{
		LEndTickCount = ::GetTickCount64() + WORKER_TICK;

		//��Ʈ��ũ ����� ó�� -> �ΰ��� �������� (��Ŷ �ڵ鷯�� ���ؼ�)
		service->GetIocpCore()->Dispatch(10);

		ThreadManager::DistributeReserveJobs();

		ThreadManager::DoGlobalQueueWork();
	}
}

int main() {

	ServerServiceRef matchService = MakeShared<ServerService>(
		NetAddress(L"127.0.0.1", 7000),
		MakeShared<IocpCore>(),
		MakeShared<MatchSession>, 1);
	ServerServiceRef gameService = MakeShared<ServerService>(
		NetAddress(L"192.168.0.2", 5000),
		MakeShared<IocpCore>(),
		MakeShared<GameSession>, 10);
	ServerServiceRef npcService = MakeShared<ServerService>(
		NetAddress(L"127.0.0.1", 4000),
		MakeShared<IocpCore>(),
		MakeShared<NpcSession>, 10);
	
	ASSERT_CRASH(matchService->Start());
	ASSERT_CRASH(gameService->Start());
	ASSERT_CRASH(npcService->Start());

	for (int i = 0; i < THREAD_SIZE; i++) {
		GThreadManager->Launch([=]()
			{
				while (true)
				{
					DoWorkerJob(gameService);
				}
			});
	}
	for (int i = 0; i < THREAD_SIZE; i++) {
		GThreadManager->Launch([=]()
			{
				while (true)
				{
					DoWorkerJob(npcService);
				}
			});
	}

	DoWorkerJob(matchService);

	GThreadManager->Join();
}