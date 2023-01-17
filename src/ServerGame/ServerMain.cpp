#include "pch.h"
#include "GameSession.h"
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

		//네트워크 입출력 처리 -> 인게임 로직까지 (패킷 핸들러에 의해서)
		service->GetIocpCore()->Dispatch(10);

		ThreadManager::DoGlobalQueueWork();
	}
}

int main() {
	ServerServiceRef service = MakeShared<ServerService>(
		NetAddress(L"127.0.0.1", 7777),
		MakeShared<IocpCore>(),
		MakeShared<GameSession>, 10);

	ASSERT_CRASH(service->Start());

	for (int i = 0; i < THREAD_SIZE; i++) {
		GThreadManager->Launch([&service]() 
			{
				while (true) 
				{
					DoWorkerJob(service);
				}
			});
	}

	GThreadManager->Join();
}