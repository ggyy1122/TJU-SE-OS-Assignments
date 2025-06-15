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

    // 更新楼层显示
    void updateFloorLabel(int floor);
    int start_x;
signals:
    // 发出请求楼层信号
    void floorRequested(int floor);

public slots:
    // 恢复按钮状态（电梯到达楼层后调用此槽）
    void restoreButtonState(int floor);
    void openDoor();
    void closeDoor();
protected:
    // 绘制事件
    void paintEvent(QPaintEvent* event) override;

private:
    int totalFloors;                       // 总楼层数
    int currentFloor;                      // 当前楼层
    QVector<bool> floorHighlight;          // 记录每层按钮的高亮状态
    QLabel* floorLabel;                    // 显示当前楼层的标签
    QVector<QPushButton*> buttons;         // 存储每层的按钮
    bool doorOpen;                         // true 表示门开着
};

#endif // ELEVATORWIDGET_H
