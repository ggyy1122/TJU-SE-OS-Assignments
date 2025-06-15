#include "Elevator.h"
#include <QMutexLocker>
#include <QDebug>
#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8") // 该指令仅支持VS环境
#endif

Elevator::Elevator(int id_, int cur_floor, QObject* parent)
    : QObject(parent), id(id_), current_floor(cur_floor), direction(IDLE) {
    // 创建定时器，每秒调用一次 step()
    timer = new QTimer(this);
    connect(timer, &QTimer::timeout, this, &Elevator::step);
    timer->start(1000);  // 每1秒调用一次 step()
}

bool Elevator::is_idle() const {
    return direction == IDLE;
}

void Elevator::add_task(int floor) {
    QMutexLocker locker(&mtx);

    if (floor > current_floor) {
        up_tasks.insert(floor);
        emit logMessage(QString("电梯 %1 响应 %2 楼的请求,上行").arg(id).arg(floor));
        qDebug() << "电梯" << id << "接收到来自" << current_floor << "楼的请求,上行。";
    }
    else if (floor < current_floor) {
        down_tasks.insert(floor);
        emit logMessage(QString("电梯 %1 接收到来自 %2 楼的请求,下行").arg(id).arg(floor));
        qDebug() << "电梯" << id << "接收到来自" << current_floor << "楼的请求,下行。";
    }
    else {
        qDebug() << "电梯" << id << "已在" << floor << "楼。";
        emit logMessage(QString("电梯 %1 当前已在 %2 楼，无需移动").arg(id).arg(floor));
        stopAndWait();
    }

    // 如果电梯当前处于空闲，重新设置方向
    if (direction == IDLE) {
        if (!up_tasks.isEmpty()) direction = UP;
        else if (!down_tasks.isEmpty()) direction = DOWN;
    }
}

void Elevator::onFloorRequested(int floor) {
    add_task(floor);  // 处理楼层请求
}

void Elevator::step() {
    if (direction == IDLE) return;

    // 移动电梯：每次移动一层
    if (direction == UP) {
        current_floor++;
        emit floorChanged(current_floor);  // 发出当前楼层信号
    }
    else if (direction == DOWN) {
        current_floor--;
        emit floorChanged(current_floor);  // 发出当前楼层信号
    }

    // 打印当前状态
    qDebug() << "电梯" << id << "当前楼层:" << current_floor
        << ", 方向:" << (direction == UP ? "向上" : "向下");

    // 到达目标楼层时，处理任务
    if (direction == UP && up_tasks.contains(current_floor)) {
        qDebug() << "电梯" << id << "到达:" << current_floor << "楼";
        emit logMessage(QString("电梯%1到达%2 楼").arg(id).arg(current_floor));
        emit arrivedAtFloor(current_floor);
        up_tasks.remove(current_floor);

        // 停留两秒后继续移动
        stopAndWait();
    }
    else if (direction == DOWN && down_tasks.contains(current_floor)) {
        qDebug() << "电梯" << id << "到达:" << current_floor << "楼";
        emit logMessage(QString("电梯%1到达%2 楼").arg(id).arg(current_floor));
        emit arrivedAtFloor(current_floor);
        down_tasks.remove(current_floor);

        // 停留两秒后继续移动
        stopAndWait();
    }

    // 更新方向
    if (direction == UP && up_tasks.isEmpty()) {
        direction = down_tasks.isEmpty() ? IDLE : DOWN;
    }
    else if (direction == DOWN && down_tasks.isEmpty()) {
        direction = up_tasks.isEmpty() ? IDLE : UP;
    }
}

// 停止电梯并等待两秒
void Elevator::stopAndWait() {
    timer->stop();

    emit openDoorRequested(); // 告诉 ElevatorWidget 开门

    QTimer::singleShot(2000, this, [=]() {
        emit closeDoorRequested(); // 告诉 ElevatorWidget 关门
        continueMovement();        // 然后继续运行
        });
}


// 恢复电梯的移动
void Elevator::continueMovement() {
    // 恢复定时器
    timer->start(1000);

    // 根据当前的任务状态继续电梯的移动
    if (direction == UP && !up_tasks.isEmpty()) {
        direction = UP;
    }
    else if (direction == DOWN && !down_tasks.isEmpty()) {
        direction = DOWN;
    }
    else {
        direction = IDLE;
    }
}
int Elevator::getMaxUpTask() const {
    if (up_tasks.isEmpty()) return -1;

    int maxFloor = std::numeric_limits<int>::min();
    for (int floor : up_tasks) {
        if (floor > maxFloor)
            maxFloor = floor;
    }
    return maxFloor;
}

int Elevator::getMinDownTask() const {
    if (down_tasks.isEmpty()) return -1;

    int minFloor = std::numeric_limits<int>::max();
    for (int floor : down_tasks) {
        if (floor < minFloor)
            minFloor = floor;
    }
    return minFloor;
}
