#include "Dispatcher.h"
#include <iostream>
#include <thread>
#include <climits>
#include<vector>
using namespace std;

Dispatcher::Dispatcher(const std::vector<Elevator*>& elevators)
    : elevators(elevators) {}

void Dispatcher::assign_task(int floor,bool isUp) {
    int best_distance = INT_MAX;
    Elevator* best_elevator = nullptr;
    vector< Elevator*>same_way_elevators;
    vector< Elevator*>empty_elevators;
    vector< Elevator*>opposite_way_elevators;
    // 优先找顺路电梯
    // 其次找空置电梯
    // 如果都是逆行电梯，选择任务最少的
    for (auto& elevator : elevators) {
        //检查是否空置
       if(elevator->is_idle())
       {
           empty_elevators.push_back(elevator);
           continue;
       }
       //否则，检查方向
       bool sameDirection=elevator->isGoingUp()==isUp;
       bool shunlu=false;
       if(sameDirection)
       {
       if(isUp&&(floor-elevator->getCurrentFloor())>0)
           shunlu=true;
       if (!isUp && (elevator->getCurrentFloor()-floor) > 0)
           shunlu = true;
       }
       //如果顺路
       if(shunlu)
       {
           same_way_elevators.push_back(elevator);
       }
       //如果不顺路
       else
       {
       opposite_way_elevators.push_back(elevator);
       }
    }
    //优先检查顺路电梯
    if(!same_way_elevators.empty())
    {
       for(auto& elevator : same_way_elevators)
       {
           int dist = abs(elevator->getCurrentFloor() - floor);
           if (dist < best_distance) {
               best_distance = dist;
               best_elevator = elevator;
           }
       }
       if (best_elevator) {
           best_elevator->add_task(floor);
           cout << "顺路任务 " << floor << " 分配给电梯 " << best_elevator->getID() << endl;
           return;
       }
    }
    //其次检查空置电梯
    if (!empty_elevators.empty())
    {
        for (auto& elevator : empty_elevators)
        {
            int dist = abs(elevator->getCurrentFloor() - floor);
            if (dist < best_distance) {
                best_distance = dist;
                best_elevator = elevator;
            }
        }
        if (best_elevator) {
            best_elevator->add_task(floor);
            cout << "空置任务 " << floor << " 分配给电梯 " << best_elevator->getID() << endl;
            return;
        }
       
    }
    //最后检查逆行电梯
    if(!opposite_way_elevators.empty())
    {
        int Count=INT_MAX;
        for (auto& elevator : opposite_way_elevators)
        {
           //如果电梯正上行
           if(elevator->isGoingUp())
           {
           if(abs(elevator->getMaxUpTask()-floor)<Count&& elevator->getMaxUpTask()!=-1)
           {
           Count= abs(elevator->getMaxUpTask() - floor);
           best_elevator=elevator;
           }
           }
           else
           {
               if (abs(floor-elevator->getMinDownTask()) < Count && elevator->getMinDownTask() != -1)
               {
                   Count = abs(elevator->getMinDownTask() - floor);
                   best_elevator = elevator;
               }
           }
        }
        if (best_elevator) {
            best_elevator->add_task(floor);
            cout << "逆行任务 " << floor << " 分配给电梯 " << best_elevator->getID() << endl;
            return;
        }
    
    }
    cout << "暂时无法分配任务 " << floor << endl;

}
