#ifndef ELEVATORWIDGET_H
#define ELEVATORWIDGET_H

#include <QWidget>
#include <QPushButton>
#include <QLabel>
#include <QVector>

class ElevatorWidget : public QWidget
{
    Q_OBJECT

public:
    explicit ElevatorWidget(int totalFloors, QWidget* parent = nullptr,int x=0);
    ~ElevatorWidget();

    // ����¥����ʾ
    void updateFloorLabel(int floor);
    int start_x;
signals:
    // ��������¥���ź�
    void floorRequested(int floor);

public slots:
    // �ָ���ť״̬�����ݵ���¥�����ô˲ۣ�
    void restoreButtonState(int floor);
    void openDoor();
    void closeDoor();
protected:
    // �����¼�
    void paintEvent(QPaintEvent* event) override;

private:
    int totalFloors;                       // ��¥����
    int currentFloor;                      // ��ǰ¥��
    QVector<bool> floorHighlight;          // ��¼ÿ�㰴ť�ĸ���״̬
    QLabel* floorLabel;                    // ��ʾ��ǰ¥��ı�ǩ
    QVector<QPushButton*> buttons;         // �洢ÿ��İ�ť
    bool doorOpen;                         // true ��ʾ�ſ���
};

#endif // ELEVATORWIDGET_H
