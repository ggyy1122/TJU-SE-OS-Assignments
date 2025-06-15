#include "Elevator.h"
#include <QMutexLocker>
#include <QDebug>
#if _MSC_VER >= 1600
#pragma execution_character_set("utf-8") // ��ָ���֧��VS����
#endif

Elevator::Elevator(int id_, int cur_floor, QObject* parent)
    : QObject(parent), id(id_), current_floor(cur_floor), direction(IDLE) {
    // ������ʱ����ÿ�����һ�� step()
    timer = new QTimer(this);
    connect(timer, &QTimer::timeout, this, &Elevator::step);
    timer->start(1000);  // ÿ1�����һ�� step()
}

bool Elevator::is_idle() const {
    return direction == IDLE;
}

void Elevator::add_task(int floor) {
    QMutexLocker locker(&mtx);

    if (floor > current_floor) {
        up_tasks.insert(floor);
        emit logMessage(QString("���� %1 ��Ӧ %2 ¥������,����").arg(id).arg(floor));
        qDebug() << "����" << id << "���յ�����" << current_floor << "¥������,���С�";
    }
    else if (floor < current_floor) {
        down_tasks.insert(floor);
        emit logMessage(QString("���� %1 ���յ����� %2 ¥������,����").arg(id).arg(floor));
        qDebug() << "����" << id << "���յ�����" << current_floor << "¥������,���С�";
    }
    else {
        qDebug() << "����" << id << "����" << floor << "¥��";
        emit logMessage(QString("���� %1 ��ǰ���� %2 ¥�������ƶ�").arg(id).arg(floor));
        stopAndWait();
    }

    // ������ݵ�ǰ���ڿ��У��������÷���
    if (direction == IDLE) {
        if (!up_tasks.isEmpty()) direction = UP;
        else if (!down_tasks.isEmpty()) direction = DOWN;
    }
}

void Elevator::onFloorRequested(int floor) {
    add_task(floor);  // ����¥������
}

void Elevator::step() {
    if (direction == IDLE) return;

    // �ƶ����ݣ�ÿ���ƶ�һ��
    if (direction == UP) {
        current_floor++;
        emit floorChanged(current_floor);  // ������ǰ¥���ź�
    }
    else if (direction == DOWN) {
        current_floor--;
        emit floorChanged(current_floor);  // ������ǰ¥���ź�
    }

    // ��ӡ��ǰ״̬
    qDebug() << "����" << id << "��ǰ¥��:" << current_floor
        << ", ����:" << (direction == UP ? "����" : "����");

    // ����Ŀ��¥��ʱ����������
    if (direction == UP && up_tasks.contains(current_floor)) {
        qDebug() << "����" << id << "����:" << current_floor << "¥";
        emit logMessage(QString("����%1����%2 ¥").arg(id).arg(current_floor));
        emit arrivedAtFloor(current_floor);
        up_tasks.remove(current_floor);

        // ͣ�����������ƶ�
        stopAndWait();
    }
    else if (direction == DOWN && down_tasks.contains(current_floor)) {
        qDebug() << "����" << id << "����:" << current_floor << "¥";
        emit logMessage(QString("����%1����%2 ¥").arg(id).arg(current_floor));
        emit arrivedAtFloor(current_floor);
        down_tasks.remove(current_floor);

        // ͣ�����������ƶ�
        stopAndWait();
    }

    // ���·���
    if (direction == UP && up_tasks.isEmpty()) {
        direction = down_tasks.isEmpty() ? IDLE : DOWN;
    }
    else if (direction == DOWN && down_tasks.isEmpty()) {
        direction = up_tasks.isEmpty() ? IDLE : UP;
    }
}

// ֹͣ���ݲ��ȴ�����
void Elevator::stopAndWait() {
    timer->stop();

    emit openDoorRequested(); // ���� ElevatorWidget ����

    QTimer::singleShot(2000, this, [=]() {
        emit closeDoorRequested(); // ���� ElevatorWidget ����
        continueMovement();        // Ȼ���������
        });
}


// �ָ����ݵ��ƶ�
void Elevator::continueMovement() {
    // �ָ���ʱ��
    timer->start(1000);

    // ���ݵ�ǰ������״̬�������ݵ��ƶ�
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
