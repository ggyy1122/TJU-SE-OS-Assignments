#ifndef DISPATCHER_H
#define DISPATCHER_H

#include "Elevator.h"
#include <vector>

class Dispatcher {
public:
    // 构造函数，接收电梯对象列表
    Dispatcher(const std::vector<Elevator*>& elevators);

    // 分配任务给最合适的电梯
    void assign_task(int floor,bool isUp);

private:
    std::vector<Elevator*> elevators; // 电梯列表，使用裸指针
};

#endif // DISPATCHER_H
