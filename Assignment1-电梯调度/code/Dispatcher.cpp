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
    // ������˳·����
    // ����ҿ��õ���
    // ����������е��ݣ�ѡ���������ٵ�
    for (auto& elevator : elevators) {
        //����Ƿ����
       if(elevator->is_idle())
       {
           empty_elevators.push_back(elevator);
           continue;
       }
       //���򣬼�鷽��
       bool sameDirection=elevator->isGoingUp()==isUp;
       bool shunlu=false;
       if(sameDirection)
       {
       if(isUp&&(floor-elevator->getCurrentFloor())>0)
           shunlu=true;
       if (!isUp && (elevator->getCurrentFloor()-floor) > 0)
           shunlu = true;
       }
       //���˳·
       if(shunlu)
       {
           same_way_elevators.push_back(elevator);
       }
       //�����˳·
       else
       {
       opposite_way_elevators.push_back(elevator);
       }
    }
    //���ȼ��˳·����
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
           cout << "˳·���� " << floor << " ��������� " << best_elevator->getID() << endl;
           return;
       }
    }
    //��μ����õ���
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
            cout << "�������� " << floor << " ��������� " << best_elevator->getID() << endl;
            return;
        }
       
    }
    //��������е���
    if(!opposite_way_elevators.empty())
    {
        int Count=INT_MAX;
        for (auto& elevator : opposite_way_elevators)
        {
           //�������������
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
            cout << "�������� " << floor << " ��������� " << best_elevator->getID() << endl;
            return;
        }
    
    }
    cout << "��ʱ�޷��������� " << floor << endl;

}
