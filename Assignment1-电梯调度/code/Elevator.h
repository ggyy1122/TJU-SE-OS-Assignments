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
    void add_task(int floor);  // �������
    int getCurrentFloor() const { return current_floor; }
    int getID()const { return id; };//����ID
    int getMaxUpTask() const;
    int getMinDownTask() const;
    bool isGoingUp()const{return direction==UP;};
public slots:
    void step();  // ÿ����µ���״̬
    void onFloorRequested(int floor);  // ����¥������

signals:
    void floorChanged(int floor);  // �����ݵ���ĳ¥��ʱ�����ź�
    void logMessage(const QString& text); // ������־��Ϣ
    void arrivedAtFloor(int floor);  // ����ĳһ¥��ʱ�����ź�
    void openDoorRequested();
    void closeDoorRequested();

private:
    int id;  // ���ݱ��
    int current_floor;  // ��ǰ¥��
    Direction direction;  // ���ݷ���
    QMutex mtx;  // ����������֤���̰߳�ȫ
    QSet<int> up_tasks;  // ��������Ŀ��¥�㣩
    QSet<int> down_tasks;  // �½�����Ŀ��¥�㣩
    QTimer* timer;  // ��ʱ�������ڶ��ڸ��µ���״̬

    void stopAndWait();  // ֹͣ���ݲ��ȴ�����
    void continueMovement();  // �ָ����ݵ��ƶ�
};

#endif // ELEVATOR_H
