#ifndef DISPATCHER_H
#define DISPATCHER_H

#include "Elevator.h"
#include <vector>

class Dispatcher {
public:
    // ���캯�������յ��ݶ����б�
    Dispatcher(const std::vector<Elevator*>& elevators);

    // �������������ʵĵ���
    void assign_task(int floor,bool isUp);

private:
    std::vector<Elevator*> elevators; // �����б�ʹ����ָ��
};

#endif // DISPATCHER_H
