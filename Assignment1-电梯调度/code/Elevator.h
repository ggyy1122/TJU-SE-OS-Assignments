#ifndef ELEVATOR_H
#define ELEVATOR_H

#include <QObject>
#include <QMutex>
#include <QSet>
#include <QTimer>

enum Direction {
    IDLE,
    UP,
    DOWN
};

class Elevator : public QObject {
    Q_OBJECT
public:
    explicit Elevator(int id_, int cur_floor, QObject* parent = nullptr);
    ~Elevator() {}

    bool is_idle() const;
    void add_task(int floor);  // 添加任务
    int getCurrentFloor() const { return current_floor; }
    int getID()const { return id; };//返回ID
    int getMaxUpTask() const;
    int getMinDownTask() const;
    bool isGoingUp()const{return direction==UP;};
public slots:
    void step();  // 每秒更新电梯状态
    void onFloorRequested(int floor);  // 处理楼层请求

signals:
    void floorChanged(int floor);  // 当电梯到达某楼层时发出信号
    void logMessage(const QString& text); // 发出日志消息
    void arrivedAtFloor(int floor);  // 到达某一楼层时发出信号
    void openDoorRequested();
    void closeDoorRequested();

private:
    int id;  // 电梯编号
    int current_floor;  // 当前楼层
    Direction direction;  // 电梯方向
    QMutex mtx;  // 互斥锁，保证多线程安全
    QSet<int> up_tasks;  // 上升任务（目标楼层）
    QSet<int> down_tasks;  // 下降任务（目标楼层）
    QTimer* timer;  // 定时器，用于定期更新电梯状态

    void stopAndWait();  // 停止电梯并等待两秒
    void continueMovement();  // 恢复电梯的移动
};

#endif // ELEVATOR_H
