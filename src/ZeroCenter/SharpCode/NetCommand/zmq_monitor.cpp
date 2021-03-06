#include "stdinc.h"
#include "net_command.h"
#pragma unmanaged
using namespace std;

typedef struct
{
	unsigned short event;
	int value;
	char address[256];
}zmq_event_t;

int read_event_msg(void* s, zmq_event_t* event)
{
	//int rc = zmq_socket_monitor(rep, "inproc://monitor.rep", ZMQ_EVENT_ALL);
	//    assert(rc == 0);
	int rc;
	zmq_msg_t msg1;  // binary part
	zmq_msg_init(&msg1);
	rc = zmq_msg_recv(&msg1, s, 0);
	if (rc == -1)
		return 1;
	const char* data = static_cast<char*>(zmq_msg_data(&msg1));
	memcpy(&(event->event), data, sizeof(event->event));
	memcpy(&(event->value), data + sizeof(event->event), sizeof(event->value));

	zmq_msg_t msg2;  //  address part
	zmq_msg_init(&msg2);
	//assert(zmq_msg_more(&msg1) != 0);
	rc = zmq_msg_recv(&msg2, s, 0);
	if (rc == -1)
		return 0;
	//assert(zmq_msg_more(&msg2) == 0);
	// copy binary data to event struct
	// copy address part
	size_t len = zmq_msg_size(&msg2);
	memcpy(event->address, zmq_msg_data(&msg2), len);
	event->address[len] = '\0';
	return 0;
}
//������
#ifdef CLIENT_COMMAND
DWORD WINAPI zmq_monitor(LPVOID arg)
{
	char * address = static_cast<char*>(arg);
#else
DWORD zmq_monitor(const char * address)
{
#endif
	zmq_event_t event;
	int rc;
	printf("starting monitor...\n");
	void* inproc = zmq_socket(get_zmq_context(), ZMQ_PAIR);
	assert(inproc);
	int iRcvTimeout = 1000;
	zmq_setsockopt(inproc, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
	rc = zmq_connect(inproc, address);
	assert(rc == 0);
	while (get_net_state() == NET_STATE_RUNING)
	{
		if (read_event_msg(inproc, &event) == 1)
			continue;
		switch (event.event)
		{
		case ZMQ_EVENT_CLOSED:
			log_debug1(DEBUG_BASE, 1, "ZMQ������%d:�����ѹر�", event.value);
			zmq_close(inproc);
			return 0;
		case ZMQ_EVENT_CLOSE_FAILED:
			log_error1("ZMQ������%d:���ӹر�ʧ��", event.value);
			zmq_close(inproc);
			return 0;
		case ZMQ_EVENT_MONITOR_STOPPED:
			log_debug1(DEBUG_BASE, 1, "ZMQ������%d:��عر�", event.value);
			zmq_close(inproc);
			return 0;
		case ZMQ_EVENT_LISTENING:
			log_debug1(DEBUG_BASE, 1, "ZMQ������%d:������������", event.value);
			break;
		case ZMQ_EVENT_BIND_FAILED:
			log_debug2(DEBUG_BASE, 1, "ZMQ������%d:�󶨶˿�ʧ��%s", event.value, event.address);
			break;
		case ZMQ_EVENT_ACCEPTED:
			log_debug2(DEBUG_BASE, 1, "ZMQ������%d:���յ�%s������", event.value, event.address);
			break;
		case ZMQ_EVENT_ACCEPT_FAILED:
			log_error2("ZMQ������%d:����%s�����ݳ���", event.value, event.address);
			break;
		case ZMQ_EVENT_CONNECTED:
			log_debug2(DEBUG_BASE, 1, "ZMQ������%d:��%s���ӳɹ�", event.value, event.address);
			break;
		case ZMQ_EVENT_CONNECT_DELAYED:
			log_debug2(DEBUG_BASE, 1, "ZMQ������%d:��%s���ӷ����ӳ�", event.value, event.address);
			break;
		case ZMQ_EVENT_CONNECT_RETRIED:
			log_debug2(DEBUG_BASE, 1, "ZMQ������%d:��������%s", event.value, event.address);
			break;
		case ZMQ_EVENT_DISCONNECTED:
			log_debug2(DEBUG_BASE, 1, "ZMQ������%d:��%s���ӹر�", event.value, event.address);
			break;
		default: break;
		}
	}
	zmq_close(inproc);
	return 0;
}